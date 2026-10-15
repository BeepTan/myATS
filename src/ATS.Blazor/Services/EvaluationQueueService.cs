using System.Text.Json;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using ATS.Blazor.Data;
using ATS.Blazor.Data.Entities;
using ATS.Blazor.Models;

namespace ATS.Blazor.Services;

public interface IEvaluationQueue
{
    ValueTask QueueEvaluationAsync(Guid applicationId);
}

public class EvaluationQueueService : BackgroundService, IEvaluationQueue
{
    private readonly Channel<Guid> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EvaluationQueueService> _logger;

    public EvaluationQueueService(IServiceProvider serviceProvider, ILogger<EvaluationQueueService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        // Unbounded channel to safely serialize evaluation requests
        _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = true
        });
    }

    public async ValueTask QueueEvaluationAsync(Guid applicationId)
    {
        await _channel.Writer.WriteAsync(applicationId);
        _logger.LogInformation("Enqueued Application {ApplicationId} for evaluation.", applicationId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EvaluationQueueService background worker started.");

        while (!stoppingToken.IsCancellationRequested && await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (_channel.Reader.TryRead(out var applicationId))
            {
                try
                {
                    await ProcessEvaluationAsync(applicationId, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process evaluation for Application {ApplicationId}", applicationId);
                }
            }
        }
    }

    private async Task ProcessEvaluationAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AtsDbContext>();
        var aiParser = scope.ServiceProvider.GetRequiredService<IAiParserService>();

        var application = await db.Applications
            .Include(a => a.Applicant)
            .Include(a => a.JobPosting)
            .Include(a => a.EvaluationResult)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application == null || application.Applicant == null || application.JobPosting == null)
        {
            _logger.LogWarning("Application {ApplicationId} or related entities not found.", applicationId);
            return;
        }

        application.Status = ApplicationStatus.Evaluating;
        await db.SaveChangesAsync(cancellationToken);

        // Deserialize stored candidate profile
        ResumeDataDto profile;
        try
        {
            profile = JsonSerializer.Deserialize<ResumeDataDto>(application.Applicant.ProfileJson) ?? new ResumeDataDto();
        }
        catch
        {
            profile = new ResumeDataDto
            {
                PersonalInfo = new PersonalInfoDto
                {
                    FullName = application.Applicant.FullName,
                    Email = application.Applicant.Email,
                    Phone = application.Applicant.Phone
                },
                Summary = application.Applicant.Summary,
                Skills = application.Applicant.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList()
            };
        }

        // Deserialize criteria
        List<EvaluationCriterionDto> criteria;
        try
        {
            criteria = JsonSerializer.Deserialize<List<EvaluationCriterionDto>>(application.JobPosting.CriteriaJson) ?? new();
        }
        catch
        {
            criteria = new();
        }

        var evalRequest = new CandidateEvaluationRequestDto
        {
            JobId = application.JobPosting.Id.ToString(),
            JobTitle = application.JobPosting.Title,
            JobDescription = application.JobPosting.Description,
            MinPassingScore = application.JobPosting.MinPassingScore,
            RequiredSkills = application.JobPosting.RequiredSkills
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList(),
            Criteria = criteria,
            CandidateProfile = profile
        };

        var evalResult = await aiParser.EvaluateCandidateAsync(evalRequest, cancellationToken);

        var categoryDict = evalResult.CategoryScores.ToDictionary(c => c.Category, c => c.Score);
        var categoryJson = JsonSerializer.Serialize(categoryDict);

        if (application.EvaluationResult != null)
        {
            // Update existing record
            application.EvaluationResult.TotalScore = evalResult.TotalScore;
            application.EvaluationResult.MinPassingScore = evalResult.MinPassingScore;
            application.EvaluationResult.PassedThreshold = evalResult.ThresholdPassed;
            application.EvaluationResult.CategoryScoresJson = categoryJson;
            application.EvaluationResult.HardRequirementsMet = evalResult.HardRequirementsMet;
            application.EvaluationResult.MissingPrerequisitesJson = JsonSerializer.Serialize(evalResult.MissingPrerequisites);
            application.EvaluationResult.StrengthsJson = JsonSerializer.Serialize(evalResult.Strengths);
            application.EvaluationResult.WeaknessesJson = JsonSerializer.Serialize(evalResult.Weaknesses);
            application.EvaluationResult.ReasoningSummary = evalResult.ReasoningSummary;
            application.EvaluationResult.EvaluationMode = evalResult.EvaluationMode;
            application.EvaluationResult.WarningMessage = evalResult.WarningMessage;
            application.EvaluationResult.EvaluatedAt = DateTime.UtcNow;
        }
        else
        {
            // Insert new record explicitly
            var resultEntity = new EvaluationResult
            {
                ApplicationId = application.Id,
                TotalScore = evalResult.TotalScore,
                MinPassingScore = evalResult.MinPassingScore,
                PassedThreshold = evalResult.ThresholdPassed,
                CategoryScoresJson = categoryJson,
                HardRequirementsMet = evalResult.HardRequirementsMet,
                MissingPrerequisitesJson = JsonSerializer.Serialize(evalResult.MissingPrerequisites),
                StrengthsJson = JsonSerializer.Serialize(evalResult.Strengths),
                WeaknessesJson = JsonSerializer.Serialize(evalResult.Weaknesses),
                ReasoningSummary = evalResult.ReasoningSummary,
                EvaluationMode = evalResult.EvaluationMode,
                WarningMessage = evalResult.WarningMessage,
                EvaluatedAt = DateTime.UtcNow
            };
            db.EvaluationResults.Add(resultEntity);
            application.EvaluationResult = resultEntity;
        }

        if (evalResult.EvaluationMode == "service_unavailable")
        {
            application.Status = ApplicationStatus.ManualReviewNeeded;
        }
        else
        {
            application.Status = evalResult.ThresholdPassed ? ApplicationStatus.Qualified : ApplicationStatus.Rejected;
        }

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Application {Id} scored: {Score} (Threshold Passed: {Passed})",
            application.Id, evalResult.TotalScore, evalResult.ThresholdPassed);
    }
}

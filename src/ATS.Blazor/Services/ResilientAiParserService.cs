using ATS.Blazor.Models;

namespace ATS.Blazor.Services;

public class ResilientAiParserService : IAiParserService
{
    private readonly FastAiClientService _fastAiClient;
    private readonly ILogger<ResilientAiParserService> _logger;

    public ResilientAiParserService(
        FastAiClientService fastAiClient,
        ILogger<ResilientAiParserService> logger)
    {
        _fastAiClient = fastAiClient;
        _logger = logger;
    }

    public async Task<ResumeDataDto> ParseResumeAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting AI Vision resume parsing via Python FastAPI microservice...");
        var result = await _fastAiClient.ExtractResumeAsync(fileStream, fileName, cancellationToken);
        _logger.LogInformation("Successfully parsed resume using {Mode}", result.ParsingMode);
        return result;
    }

    public async Task<CandidateEvaluationResponseDto> EvaluateCandidateAsync(CandidateEvaluationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting AI Rubric evaluation via Python FastAPI microservice...");
            var result = await _fastAiClient.EvaluateCandidateAsync(request, cancellationToken);
            _logger.LogInformation("Successfully evaluated candidate. Total score: {Score}", result.TotalScore);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FastAPI evaluation service unreachable or error encountered. Pausing automated evaluation for manual review/retry.");
            return new CandidateEvaluationResponseDto
            {
                JobId = request.JobId,
                TotalScore = 0,
                MinPassingScore = request.MinPassingScore,
                ThresholdPassed = false,
                HardRequirementsMet = false,
                EvaluationMode = "service_unavailable",
                WarningMessage = "AI evaluation microservice is currently unreachable or encountered an API error. Automated scoring was skipped for manual recruiter review.",
                ReasoningSummary = "AI Evaluation was paused because the microservice was offline or encountered an error. No offline fallback was used."
            };
        }
    }
}

using System.Net.Http.Json;
using ATS.Blazor.Models;

namespace ATS.Blazor.Services;

public class FastAiClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FastAiClientService> _logger;

    public FastAiClientService(HttpClient httpClient, ILogger<FastAiClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResumeDataDto> ExtractResumeAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync("/api/v1/parser/extract", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ResumeDataDto>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Empty response received from parser service.");
    }

    public async Task<CandidateEvaluationResponseDto> EvaluateCandidateAsync(CandidateEvaluationRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/evaluator/score", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CandidateEvaluationResponseDto>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Empty response received from evaluator service.");
    }

    public async Task<AiServiceHealthDto> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(4));

            var response = await _httpClient.GetAsync("/health", cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var health = await response.Content.ReadFromJsonAsync<AiServiceHealthDto>(cancellationToken: cts.Token);
                if (health != null)
                {
                    return health;
                }
            }

            return new AiServiceHealthDto
            {
                Service = "offline",
                OpenAiKeyStatus = "unknown",
                OpenAiKeyValid = false,
                Message = $"Microservice responded with status {(int)response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Health check probe to Python AI microservice failed.");
            return new AiServiceHealthDto
            {
                Service = "offline",
                OpenAiKeyStatus = "offline",
                OpenAiKeyValid = false,
                Message = "Microservice offline: Unable to reach Python service on port 8000"
            };
        }
    }
}

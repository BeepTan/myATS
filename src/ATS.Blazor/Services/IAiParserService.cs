using ATS.Blazor.Models;

namespace ATS.Blazor.Services;

public interface IAiParserService
{
    Task<ResumeDataDto> ParseResumeAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<CandidateEvaluationResponseDto> EvaluateCandidateAsync(CandidateEvaluationRequestDto request, CancellationToken cancellationToken = default);
}

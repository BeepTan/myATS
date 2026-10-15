using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATS.Blazor.Data.Entities;

public enum ApplicationStatus
{
    PendingEvaluation,
    Evaluating,
    Qualified,
    Rejected,
    ManualReviewNeeded
}

public class Application
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid JobPostingId { get; set; }
    [ForeignKey(nameof(JobPostingId))]
    public JobPosting? JobPosting { get; set; }

    [Required]
    public Guid ApplicantId { get; set; }
    [ForeignKey(nameof(ApplicantId))]
    public Applicant? Applicant { get; set; }

    public string ResumeFileName { get; set; } = string.Empty;
    public string ResumeFilePath { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.PendingEvaluation;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public EvaluationResult? EvaluationResult { get; set; }
}

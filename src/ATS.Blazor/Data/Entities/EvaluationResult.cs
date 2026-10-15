using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATS.Blazor.Data.Entities;

public class EvaluationResult
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ApplicationId { get; set; }
    [ForeignKey(nameof(ApplicationId))]
    public Application? Application { get; set; }

    /// <summary>
    /// Composite score (0-100).
    /// </summary>
    public double TotalScore { get; set; }

    public double MinPassingScore { get; set; }
    public bool PassedThreshold { get; set; }

    /// <summary>
    /// Category scores dictionary serialized as JSON.
    /// </summary>
    public string CategoryScoresJson { get; set; } = "{}";

    public bool HardRequirementsMet { get; set; } = true;
    public string MissingPrerequisitesJson { get; set; } = "[]";
    public string StrengthsJson { get; set; } = "[]";
    public string WeaknessesJson { get; set; } = "[]";

    public string ReasoningSummary { get; set; } = string.Empty;

    /// <summary>
    /// ai_rubric or heuristic_fallback
    /// </summary>
    public string EvaluationMode { get; set; } = "ai_rubric";

    public string? WarningMessage { get; set; }

    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

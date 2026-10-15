using System.ComponentModel.DataAnnotations;

namespace ATS.Blazor.Data.Entities;

public class JobPosting
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Department { get; set; } = "Engineering";

    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of mandatory skills required for the role.
    /// </summary>
    public string RequiredSkills { get; set; } = string.Empty;

    /// <summary>
    /// Minimum passing cutoff score (0-100) set by Admin.
    /// </summary>
    public double MinPassingScore { get; set; } = 70.0;

    /// <summary>
    /// JSON array of evaluation criteria and category weights.
    /// </summary>
    public string CriteriaJson { get; set; } = "[]";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Application> Applications { get; set; } = new();
}

using System.ComponentModel.DataAnnotations;

namespace ATS.Blazor.Data.Entities;

public class Applicant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    public string LinkedInUrl { get; set; } = string.Empty;
    public string GitHubUrl { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of candidate skills.
    /// </summary>
    public string Skills { get; set; } = string.Empty;

    /// <summary>
    /// Full structured JSON of the applicant's profile.
    /// </summary>
    public string ProfileJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Application> Applications { get; set; } = new();
}

using System.Text.Json.Serialization;

namespace ATS.Blazor.Models;

public class PersonalInfoDto
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("linkedin_url")]
    public string LinkedInUrl { get; set; } = string.Empty;

    [JsonPropertyName("github_url")]
    public string GitHubUrl { get; set; } = string.Empty;
}

public class WorkExperienceDto
{
    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("job_title")]
    public string JobTitle { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("end_date")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("responsibilities")]
    public List<string> Responsibilities { get; set; } = new();
}

public class EducationDto
{
    [JsonPropertyName("institution")]
    public string Institution { get; set; } = string.Empty;

    [JsonPropertyName("degree")]
    public string Degree { get; set; } = string.Empty;

    [JsonPropertyName("graduation_year")]
    public string GraduationYear { get; set; } = string.Empty;
}

public class ResumeDataDto
{
    [JsonPropertyName("personal_info")]
    public PersonalInfoDto PersonalInfo { get; set; } = new();

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("work_experience")]
    public List<WorkExperienceDto> WorkExperience { get; set; } = new();

    [JsonPropertyName("education")]
    public List<EducationDto> Education { get; set; } = new();

    [JsonPropertyName("skills")]
    public List<string> Skills { get; set; } = new();

    [JsonPropertyName("certifications")]
    public List<string> Certifications { get; set; } = new();

    [JsonPropertyName("parsing_mode")]
    public string ParsingMode { get; set; } = "ai_vision";

    [JsonPropertyName("warning_message")]
    public string? WarningMessage { get; set; }
}

public class EvaluationCriterionDto
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("weight_percentage")]
    public double WeightPercentage { get; set; }

    [JsonPropertyName("instructions")]
    public string Instructions { get; set; } = string.Empty;
}

public class CandidateEvaluationRequestDto
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("job_title")]
    public string JobTitle { get; set; } = string.Empty;

    [JsonPropertyName("job_description")]
    public string JobDescription { get; set; } = string.Empty;

    [JsonPropertyName("min_passing_score")]
    public double MinPassingScore { get; set; } = 70.0;

    [JsonPropertyName("required_skills")]
    public List<string> RequiredSkills { get; set; } = new();

    [JsonPropertyName("criteria")]
    public List<EvaluationCriterionDto> Criteria { get; set; } = new();

    [JsonPropertyName("candidate_profile")]
    public ResumeDataDto CandidateProfile { get; set; } = new();
}

public class CategoryScoreItemDto
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public double Score { get; set; }
}

public class CandidateEvaluationResponseDto
{
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("total_score")]
    public double TotalScore { get; set; }

    [JsonPropertyName("min_passing_score")]
    public double MinPassingScore { get; set; }

    [JsonPropertyName("threshold_passed")]
    public bool ThresholdPassed { get; set; }

    [JsonPropertyName("category_scores")]
    public List<CategoryScoreItemDto> CategoryScores { get; set; } = new();

    [JsonPropertyName("hard_requirements_met")]
    public bool HardRequirementsMet { get; set; } = true;

    [JsonPropertyName("missing_prerequisites")]
    public List<string> MissingPrerequisites { get; set; } = new();

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = new();

    [JsonPropertyName("reasoning_summary")]
    public string ReasoningSummary { get; set; } = string.Empty;

    [JsonPropertyName("evaluation_mode")]
    public string EvaluationMode { get; set; } = "ai_rubric";

    [JsonPropertyName("warning_message")]
    public string? WarningMessage { get; set; }
}

public class AiServiceHealthDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "unknown";

    [JsonPropertyName("service")]
    public string Service { get; set; } = "offline";

    [JsonPropertyName("openai_key_status")]
    public string OpenAiKeyStatus { get; set; } = "unknown"; // "valid_format", "invalid_format", "missing", "offline"

    [JsonPropertyName("openai_key_valid")]
    public bool OpenAiKeyValid { get; set; }

    [JsonPropertyName("openai_key_prefix")]
    public string? OpenAiKeyPrefix { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonIgnore]
    public bool IsOnline => string.Equals(Service, "online", StringComparison.OrdinalIgnoreCase);

    [JsonIgnore]
    public bool HasApiKeyIssue => OpenAiKeyStatus == "missing" || OpenAiKeyStatus == "invalid_format";
}

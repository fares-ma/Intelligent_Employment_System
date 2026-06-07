namespace Services.Abstractions.DTOs.JobPosting;

/// <summary>
/// DTO for displaying job posting details
/// </summary>
public class JobPostingDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string? SalaryRange { get; set; }
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;

    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public DateTime? ApplicationDeadline { get; set; }

    public List<string> RequiredSkills { get; set; } = new();
    public int ApplicationCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── New fields ──

    public string? Department { get; set; }
    public decimal? GPA { get; set; }
    public string? GPAPriority { get; set; }
    public int? ExperienceMinYears { get; set; }
    public int? ExperienceMaxYears { get; set; }
    public string? ExperiencePriority { get; set; }

    public List<JobDegreeDto> Degrees { get; set; } = new();
    public List<JobRoleDto> Roles { get; set; } = new();
    public List<JobSkillDto> Skills { get; set; } = new();
}

namespace Services.Abstractions.DTOs.JobPosting;

/// <summary>
/// DTO for creating a new job posting — matches the frontend form structure
/// </summary>
public class CreateJobPostingDto
{
    // ── Legacy fields (optional for backward compatibility) ──

    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? SalaryRange { get; set; }
    public string? Location { get; set; }
    public DateTime? ApplicationDeadline { get; set; }

    /// <summary>
    /// Legacy: list of existing skill IDs. Ignored when Skills list is provided.
    /// </summary>
    public List<int> RequiredSkillIds { get; set; } = new();

    // ── New frontend fields ──

    public string? Department { get; set; }
    public string? EmploymentType { get; set; }

    public decimal? GPA { get; set; }
    public string? GPAPriority { get; set; }

    public int? ExperienceMinYears { get; set; }
    public int? ExperienceMaxYears { get; set; }
    public string? ExperiencePriority { get; set; }

    public List<JobDegreeDto> Degrees { get; set; } = new();
    public List<JobRoleDto> Roles { get; set; } = new();
    public List<JobSkillDto> Skills { get; set; } = new();
}

public class JobDegreeDto
{
    public string DegreeName { get; set; } = string.Empty;
    public string DegreePriority { get; set; } = "None";
}

public class JobRoleDto
{
    public string RoleName { get; set; } = string.Empty;
    public string RolePriority { get; set; } = "None";
}

public class JobSkillDto
{
    public string SkillName { get; set; } = string.Empty;
    public string SkillPriority { get; set; } = "None";
}

using Domain.Enums;

namespace Domain.Models;

/// <summary>
/// Job opportunity posted by a company.
/// </summary>
public class JobPost
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? AiProcessedDescription { get; set; }

    public string? Location { get; set; }

    public JobType JobType { get; set; }

    public WorkLocation WorkLocation { get; set; }

    public JobLevel CareerLevel { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string? Currency { get; set; } = "EGP";

    public DateTime? ExpiryDate { get; set; }

    public bool IsPublished { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public string Status { get; set; } = "ACTIVE";

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string CreatedByRecruiterId { get; set; } = string.Empty;
    public Recruiter CreatedByRecruiter { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // ── New fields for frontend job creation form ──

    /// <summary>
    /// Department name (e.g. "Product", "Engineering")
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Minimum GPA requirement (0-4)
    /// </summary>
    public decimal? GPA { get; set; }

    public string? GPAPriority { get; set; }

    public int? ExperienceMinYears { get; set; }

    public int? ExperienceMaxYears { get; set; }

    public string? ExperiencePriority { get; set; }

    /// <summary>
    /// Required degrees stored as JSON array: [{"degreeName":"...","degreePriority":"..."}]
    /// </summary>
    public string? DegreesJson { get; set; }

    /// <summary>
    /// Required roles stored as JSON array: [{"roleName":"...","rolePriority":"..."}]
    /// </summary>
    public string? RolesJson { get; set; }

    /// <summary>
    /// Required skills stored as JSON array: [{"skillName":"...","skillPriority":"..."}]
    /// </summary>
    public string? SkillsJson { get; set; }

    // Navigation properties
    public ICollection<JobPostSkill> JobPostSkills { get; set; } = new List<JobPostSkill>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
}

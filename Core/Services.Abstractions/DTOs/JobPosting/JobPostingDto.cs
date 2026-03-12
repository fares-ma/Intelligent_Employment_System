namespace Services.Abstractions.DTOs.JobPosting;

/// <summary>
/// DTO for displaying job posting details
/// </summary>
public class JobPostingDto
{
    public int Id { get; set; }

    /// <summary>
    /// Job title (e.g., Senior Software Engineer)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Full job description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Required qualifications and experience
    /// </summary>
    public string Requirements { get; set; } = string.Empty;

    /// <summary>
    /// Salary range (e.g., "50000-80000")
    /// </summary>
    public string? SalaryRange { get; set; }

    /// <summary>
    /// Job location (can be remote)
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Employment type (Full-time, Part-time, Contract, etc.)
    /// </summary>
    public string EmploymentType { get; set; } = string.Empty;

    /// <summary>
    /// Company ID who posted this job
    /// </summary>
    public int CompanyId { get; set; }

    /// <summary>
    /// Company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Is the job posting currently active and accepting applications
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Application deadline
    /// </summary>
    public DateTime? ApplicationDeadline { get; set; }

    /// <summary>
    /// Required skills for this position
    /// </summary>
    public List<string> RequiredSkills { get; set; } = new();

    /// <summary>
    /// Number of applications received
    /// </summary>
    public int ApplicationCount { get; set; }

    /// <summary>
    /// When the job was posted
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update time
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

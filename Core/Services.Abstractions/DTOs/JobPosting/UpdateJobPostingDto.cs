namespace Services.Abstractions.DTOs.JobPosting;

/// <summary>
/// DTO for updating an existing job posting
/// </summary>
public class UpdateJobPostingDto
{
    /// <summary>
    /// Job title (required, max 100 characters)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Full job description (required, max 5000 characters)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Required qualifications (required, max 2000 characters)
    /// </summary>
    public string Requirements { get; set; } = string.Empty;

    /// <summary>
    /// Salary range - optional (max 50 characters)
    /// </summary>
    public string? SalaryRange { get; set; }

    /// <summary>
    /// Job location (required, max 100 characters)
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Employment type (required, max 50 characters)
    /// </summary>
    public string EmploymentType { get; set; } = string.Empty;

    /// <summary>
    /// Application deadline - optional
    /// </summary>
    public DateTime? ApplicationDeadline { get; set; }

    /// <summary>
    /// Is the job posting active (null means no change)
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// List of required skill IDs for this position (optional)
    /// </summary>
    public List<int> RequiredSkillIds { get; set; } = new();
}

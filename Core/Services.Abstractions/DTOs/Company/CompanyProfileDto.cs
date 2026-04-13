namespace Services.Abstractions.DTOs.Company;

/// <summary>
/// DTO for Company profile information (read-only)
/// Used when retrieving company details
/// </summary>
public class CompanyProfileDto
{
    /// <summary>
    /// Unique identifier of the company
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Company name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique tax/registration number
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// Industry sector (e.g., Technology, Finance, Healthcare)
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Company website URL
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Company description or bio
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Stored path for company logo / brand image (relative to file storage root when applicable).
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>
    /// Number of active job posts
    /// </summary>
    public int ActiveJobPostsCount { get; set; }

    /// <summary>
    /// Number of recruiters in the company
    /// </summary>
    public int RecruiterCount { get; set; }

    /// <summary>
    /// ID of the company admin/owner
    /// </summary>
    public string AdminId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the company admin/owner
    /// </summary>
    public string AdminName { get; set; } = string.Empty;

    /// <summary>
    /// When the company was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

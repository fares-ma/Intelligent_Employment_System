namespace Services.Abstractions.DTOs.Candidate;

/// <summary>
/// DTO for displaying candidate experience record
/// </summary>
public class CandidateExperienceDto
{
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Job title (e.g., Senior Developer, Product Manager)
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Name of the company
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Description of duties and achievements
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Start date of employment
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of employment (null if currently working)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Whether this is a current position
    /// </summary>
    public bool IsCurrent => EndDate is null;

    /// <summary>
    /// When this experience record was added
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

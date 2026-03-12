namespace Services.Abstractions.DTOs.Candidate;

/// <summary>
/// DTO for creating a new experience record
/// </summary>
public class CreateExperienceDto
{
    /// <summary>
    /// Job title (required, 1-100 characters)
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Company name (required, 1-100 characters)
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Job description (optional, max 1000 characters)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Start date of employment (required)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of employment (optional, null if currently working)
    /// </summary>
    public DateTime? EndDate { get; set; }
}

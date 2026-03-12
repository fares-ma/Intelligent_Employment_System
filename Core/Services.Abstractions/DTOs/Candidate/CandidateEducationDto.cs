namespace Services.Abstractions.DTOs.Candidate;

/// <summary>
/// DTO for displaying candidate education record
/// </summary>
public class CandidateEducationDto
{
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of degree (e.g., Bachelor's, Master's, PhD)
    /// </summary>
    public string Degree { get; set; } = string.Empty;

    /// <summary>
    /// Field of study (e.g., Computer Science, Business Administration)
    /// </summary>
    public string FieldOfStudy { get; set; } = string.Empty;

    /// <summary>
    /// Name of the educational institution
    /// </summary>
    public string Institution { get; set; } = string.Empty;

    /// <summary>
    /// Year of graduation (e.g., 2020)
    /// </summary>
    public int GraduationYear { get; set; }

    /// <summary>
    /// When this education record was added
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

namespace Services.Abstractions.DTOs.Candidate;

/// <summary>
/// DTO for updating an existing education record
/// </summary>
public class UpdateEducationDto
{
    /// <summary>
    /// Type of degree (required, 1-50 characters)
    /// </summary>
    public string Degree { get; set; } = string.Empty;

    /// <summary>
    /// Field of study (required, 1-100 characters)
    /// </summary>
    public string FieldOfStudy { get; set; } = string.Empty;

    /// <summary>
    /// Name of the institution (required, 1-100 characters)
    /// </summary>
    public string Institution { get; set; } = string.Empty;

    /// <summary>
    /// Year of graduation (required, 1900-2100)
    /// </summary>
    public int GraduationYear { get; set; }
}

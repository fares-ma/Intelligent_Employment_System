namespace Services.Abstractions.DTOs.Company;

/// <summary>
/// DTO for updating company information
/// Only admin can update company details
/// </summary>
public class UpdateCompanyDto
{
    /// <summary>
    /// Company name (required, 2-100 characters)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Industry sector (optional, e.g., Technology, Finance)
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Company website URL (optional, must be valid URL if provided)
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Company description or bio (optional, max 1000 characters)
    /// </summary>
    public string? Description { get; set; }
}

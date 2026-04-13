namespace Services.Abstractions.DTOs.Company;

/// <summary>
/// Payload for creating a company (used with multipart form or JSON).
/// </summary>
public class CreateCompanyRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Industry { get; set; } = string.Empty;

    public string? Website { get; set; }

    public string TaxNumber { get; set; } = string.Empty;

    public string? Description { get; set; }
}

namespace Services.Abstractions.DTOs.Auth;

/// <summary>
/// Register a company + Admin recruiter (one-step onboarding)
/// </summary>
public class RegisterCompanyRequestDto
{
    // Personal info
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Gender { get; set; } // "Male", "Female"
    public required DateTime DateOfBirth { get; set; }

    // Company info
    public required string CompanyName { get; set; }
    public string? TaxNumber { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
}

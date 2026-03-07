namespace Services.Abstractions.DTOs.Candidates;

public class UpdateCandidateProfileDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; } // enum string
    public DateTime? DateOfBirth { get; set; }

    public string? JobTitle { get; set; }
    public string? Summary { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? CareerLevel { get; set; } // enum string
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

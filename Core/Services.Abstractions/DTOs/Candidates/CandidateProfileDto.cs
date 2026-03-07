namespace Services.Abstractions.DTOs.Candidates;

public class CandidateProfileDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePicturePath { get; set; }

    public string? JobTitle { get; set; }
    public string? Summary { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? CareerLevel { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public List<SkillDto> Skills { get; set; } = new();
    public List<ResumeDto> Resumes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

using Services.Abstractions.DTOs.Candidate;
using Services.Abstractions.DTOs.Candidates;

namespace Services.Abstractions.DTOs.Candidate;

public class RecruiterCandidateProfileDto
{
    // Candidate Info
    public string CandidateId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? ResumeUrl { get; set; }
    public string? LinkedInUrl { get; set; }

    // Professional Info
    public int YearsOfExperience { get; set; }
    public List<CandidateSkillDto> Skills { get; set; } = new List<CandidateSkillDto>();
    public List<CandidateExperienceDto> Experience { get; set; } = new List<CandidateExperienceDto>();
    public List<CandidateEducationDto> Education { get; set; } = new List<CandidateEducationDto>();
    public List<ResumeDto> Resumes { get; set; } = new List<ResumeDto>();
}

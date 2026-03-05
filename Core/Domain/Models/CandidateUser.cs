using Domain.Enums;

namespace Domain.Models;

/// <summary>
/// Job seeker — inherits ApplicationUser (TPH). Maintains profile, skills, resumes.
/// </summary>
public class CandidateUser : ApplicationUser
{
    public string? JobTitle { get; set; }

    public string? Summary { get; set; }

    public int? YearsOfExperience { get; set; }

    public JobLevel? CareerLevel { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    // Navigation properties
    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public ICollection<CandidateAssessment> CandidateAssessments { get; set; } = new List<CandidateAssessment>();
}

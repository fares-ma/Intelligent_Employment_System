using Domain.Enums;

namespace Domain.Models;

/// <summary>
/// A candidate's application to a specific job — tracks pipeline stage, AI score, rating.
/// </summary>
public class JobApplication
{
    public int Id { get; set; }

    public string CandidateId { get; set; } = string.Empty;
    public CandidateUser Candidate { get; set; } = null!;

    public int JobPostId { get; set; }
    public JobPost JobPost { get; set; } = null!;

    public int ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    /// <summary>
    /// AI match score 0.00–100.00. Null = pending AI scoring.
    /// </summary>
    public decimal? MatchScore { get; set; }

    public string? MatchReport { get; set; }

    /// <summary>
    /// Recruiter star rating 1–5.
    /// </summary>
    public int? RecruiterRating { get; set; }

    public string? RecruiterNotes { get; set; }

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<CandidateAssessment> CandidateAssessments { get; set; } = new List<CandidateAssessment>();
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}

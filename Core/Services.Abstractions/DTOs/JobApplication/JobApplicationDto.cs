namespace Services.Abstractions.DTOs.JobApplication;

/// <summary>
/// DTO for displaying job application details
/// </summary>
public class JobApplicationDto
{
    public int Id { get; set; }

    /// <summary>
    /// ID of the candidate who applied
    /// </summary>
    public string CandidateId { get; set; } = string.Empty;

    /// <summary>
    /// Candidate username
    /// </summary>
    public string CandidateName { get; set; } = string.Empty;

    /// <summary>
    /// ID of the job posting
    /// </summary>
    public int JobPostId { get; set; }

    /// <summary>
    /// Job posting title
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the application (Pending, UnderReview, Assessment, Interview, Accepted, Rejected, Withdrawn)
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Cover letter (read-only for backward compatibility)
    /// </summary>
    public string? CoverLetter { get; set; }

    /// <summary>
    /// Resume ID attached to this application (required, but nullable in response)
    /// </summary>
    public int? ResumeId { get; set; }

    /// <summary>
    /// When the application was submitted
    /// </summary>
    public DateTime AppliedAt { get; set; }

    /// <summary>
    /// Last update time
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Recruiter notes or rejection reason
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// AI matching score for the application (0-100)
    /// </summary>
    public decimal? MatchScore { get; set; }

    /// <summary>
    /// Recruiter rating (1-5 stars)
    /// </summary>
    public int? RecruiterRating { get; set; }
}

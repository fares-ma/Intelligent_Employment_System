namespace Services.Abstractions.DTOs.JobApplication;

/// <summary>
/// DTO for updating job application status (recruiter/admin only)
/// </summary>
public class UpdateJobApplicationDto
{
    /// <summary>
    /// New status for the application (Pending, UnderReview, Assessment, Interview, Accepted, Rejected, Withdrawn)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Recruiter notes (stored as RecruiterNotes, max 500 characters)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Recruiter rating (1-5 stars)
    /// </summary>
    public int? RecruiterRating { get; set; }
}

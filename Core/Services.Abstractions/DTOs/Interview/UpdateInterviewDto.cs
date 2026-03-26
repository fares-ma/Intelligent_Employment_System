namespace Services.Abstractions.DTOs.Interview;

/// <summary>
/// DTO for updating interview status and completing interviews
/// </summary>
public class UpdateInterviewDto
{
    /// <summary>
    /// New status: "Scheduled", "InProgress", "Completed", or "Cancelled" (required)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's written answers (optional, for completed AI interviews)
    /// </summary>
    public string? AiAnswers { get; set; }

    /// <summary>
    /// Interview score 0-100 (optional, for completed interviews)
    /// </summary>
    public decimal? Score { get; set; }

    /// <summary>
    /// Feedback notes from interviewer (optional, max 1000 characters)
    /// </summary>
    public string? FeedbackNotes { get; set; }

    /// <summary>
    /// Updated meeting link (optional)
    /// </summary>
    public string? MeetingLink { get; set; }
}

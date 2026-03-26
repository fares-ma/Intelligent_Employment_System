namespace Services.Abstractions.DTOs.Interview;

/// <summary>
/// DTO for displaying interview details
/// </summary>
public class InterviewDto
{
    /// <summary>
    /// Unique identifier for the interview
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the job application this interview is for
    /// </summary>
    public int JobApplicationId { get; set; }

    /// <summary>
    /// ID of the candidate being interviewed
    /// </summary>
    public string CandidateId { get; set; } = string.Empty;

    /// <summary>
    /// Candidate's username
    /// </summary>
    public string CandidateName { get; set; } = string.Empty;

    /// <summary>
    /// Job posting title
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Type of interview (AI or Live)
    /// </summary>
    public string InterviewType { get; set; } = "Live";

    /// <summary>
    /// Current status (Scheduled, InProgress, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Scheduled";

    /// <summary>
    /// Scheduled start time (UTC)
    /// </summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int DurationMinutes { get; set; } = 30;

    /// <summary>
    /// Meeting link or video call URL
    /// </summary>
    public string? MeetingLink { get; set; }

    /// <summary>
    /// AI-generated questions (JSON format, if applicable)
    /// </summary>
    public string? AiQuestions { get; set; }

    /// <summary>
    /// Candidate's written answers (JSON format, for AI interviews)
    /// </summary>
    public string? AiAnswers { get; set; }

    /// <summary>
    /// Interview score (0-100)
    /// </summary>
    public decimal? Score { get; set; }

    /// <summary>
    /// Feedback notes from interviewer
    /// </summary>
    public string? FeedbackNotes { get; set; }

    /// <summary>
    /// When the interview was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// When the interview was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

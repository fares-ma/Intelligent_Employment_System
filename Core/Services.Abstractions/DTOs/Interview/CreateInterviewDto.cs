namespace Services.Abstractions.DTOs.Interview;

/// <summary>
/// DTO for creating a new interview
/// </summary>
public class CreateInterviewDto
{
    /// <summary>
    /// ID of the job application to schedule interview for (required)
    /// </summary>
    public int JobApplicationId { get; set; }

    /// <summary>
    /// Type of interview: "AI" or "Live" (required)
    /// </summary>
    public string InterviewType { get; set; } = "Live";

    /// <summary>
    /// When to schedule the interview (UTC, must be in future)
    /// </summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// Interview duration in minutes (default: 30)
    /// </summary>
    public int DurationMinutes { get; set; } = 30;

    /// <summary>
    /// Meeting link or video call URL (optional, required for Live interviews)
    /// </summary>
    public string? MeetingLink { get; set; }

    /// <summary>
    /// AI-generated questions in JSON format (optional, for AI interviews)
    /// </summary>
    public string? AiQuestions { get; set; }
}

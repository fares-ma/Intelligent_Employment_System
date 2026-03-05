using Domain.Enums;

namespace Domain.Models;

/// <summary>
/// Scheduled evaluation meeting — AI voice or live video.
/// </summary>
public class Interview
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public InterviewType InterviewType { get; set; }

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public DateTime ScheduledAt { get; set; }

    public int DurationMinutes { get; set; } = 30;

    public string? MeetingLink { get; set; }

    /// <summary>
    /// JSON — AI-generated questions.
    /// </summary>
    public string? AiQuestions { get; set; }

    /// <summary>
    /// AI interview transcript.
    /// </summary>
    public string? AiTranscript { get; set; }

    public decimal? Score { get; set; }

    public string? FeedbackNotes { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

namespace Services.Abstractions.DTOs.Interview;

public class RecruiterInterviewDto
{
    // Interview Info
    public int InterviewId { get; set; }
    public string InterviewType { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
    public string? Notes { get; set; }
    public decimal? Rating { get; set; }

    // Candidate Info
    public string CandidateId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

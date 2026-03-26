namespace Services.Abstractions.DTOs.Interview;

public class AiInterviewQuestionsDto
{
    public int InterviewId { get; set; }
    public string Questions { get; set; } = string.Empty;
}

public class SubmitAiInterviewDto
{
    public int InterviewId { get; set; }
    public string Answers { get; set; } = string.Empty;
}

public class CompleteInterviewDto
{
    public int InterviewId { get; set; }
    public decimal? Score { get; set; }
    public string? FeedbackNotes { get; set; }
}

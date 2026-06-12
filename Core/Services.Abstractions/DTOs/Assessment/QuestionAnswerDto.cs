namespace Services.Abstractions.DTOs.Assessment;

public class QuestionAnswerDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string? CandidateAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int Points { get; set; }
}

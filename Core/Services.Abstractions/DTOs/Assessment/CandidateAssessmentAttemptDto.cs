namespace Services.Abstractions.DTOs.Assessment;

public class CandidateAssessmentAttemptDto
{
    public int CandidateAssessmentId { get; set; }
    
    public DateTime StartedAt { get; set; }
    
    public DateTime DeadlineAt { get; set; }
    
    // Questions are served without CorrectAnswer
    public List<AssessmentQuestionDto> Questions { get; set; } = new();
}

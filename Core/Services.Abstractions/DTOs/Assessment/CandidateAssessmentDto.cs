namespace Services.Abstractions.DTOs.Assessment;

public class CandidateAssessmentDto
{
    public int Id { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public int AssessmentId { get; set; }
    public int JobApplicationId { get; set; }
    public string? Answers { get; set; }
    public decimal? Score { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool IsCompleted { get; set; }
    
    public AssessmentDto Assessment { get; set; } = null!;
}

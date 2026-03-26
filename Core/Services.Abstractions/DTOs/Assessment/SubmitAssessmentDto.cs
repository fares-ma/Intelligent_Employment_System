using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.Assessment;

public class SubmitAssessmentDto
{
    [Required]
    public List<CandidateAnswerDto> Answers { get; set; } = new();
}

public class CandidateAnswerDto
{
    [Required]
    public int QuestionId { get; set; }
    
    [Required]
    public string Answer { get; set; } = string.Empty;
}

public class SubmitAssessmentResultDto
{
    public int CandidateAssessmentId { get; set; }
    public decimal? Score { get; set; }
    public int TotalScore { get; set; }
    public bool IsCompleted { get; set; }
}

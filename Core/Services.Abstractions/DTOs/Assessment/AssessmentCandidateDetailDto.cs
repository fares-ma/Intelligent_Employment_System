namespace Services.Abstractions.DTOs.Assessment;

public class AssessmentCandidateDetailDto
{
    // Candidate Info
    public string CandidateId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ResumeUrl { get; set; }

    // Assessment Info
    public int AssessmentId { get; set; }
    public string AssessmentName { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public int MaximumScore { get; set; }
    public DateTime? SubmissionDate { get; set; }

    // Questions and Answers
    public List<QuestionAnswerDto> Questions { get; set; } = new List<QuestionAnswerDto>();
}

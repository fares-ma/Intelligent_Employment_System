namespace Services.Abstractions.DTOs.Assessment;

public class AssessmentCandidateListDto
{
    public string CandidateId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmissionDate { get; set; }
}

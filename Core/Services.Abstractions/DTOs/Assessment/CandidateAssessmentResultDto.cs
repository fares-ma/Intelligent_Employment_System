using Services.Abstractions.DTOs.JobApplication; // Reusing ApplicantCandidateDto

namespace Services.Abstractions.DTOs.Assessment;

public class CandidateAssessmentResultDto
{
    public int CandidateAssessmentId { get; set; }
    
    public ApplicantCandidateDto Candidate { get; set; } = new();
    
    public decimal? Score { get; set; }
    
    public int TotalScore { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public bool IsCompleted { get; set; }
}

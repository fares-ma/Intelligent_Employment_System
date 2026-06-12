namespace Services.Abstractions.DTOs.JobPosting;

public class JobApplicantDto
{
    public string CandidateId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApplicationStatus { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public decimal? MatchScore { get; set; }
}

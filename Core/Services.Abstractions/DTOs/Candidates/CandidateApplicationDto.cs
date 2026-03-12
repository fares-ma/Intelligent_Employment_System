namespace Services.Abstractions.DTOs.Candidates;

public class CandidateApplicationDto
{
    public int ApplicationId { get; set; }
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string Status { get; set; } = null!; // ApplicationStatus enum
    public decimal? MatchScore { get; set; }
    public string? MatchReport { get; set; }
    public DateTime AppliedAt { get; set; }
}

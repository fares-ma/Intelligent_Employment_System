namespace Domain.Models;

public class CandidateExperience
{
    public int Id { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public CandidateUser Candidate { get; set; } = null!;
    public string JobTitle { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // max 500 chars
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

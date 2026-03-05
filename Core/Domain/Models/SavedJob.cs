namespace Domain.Models;

/// <summary>
/// Junction entity: CandidateUser ↔ JobPost bookmark. Composite PK (CandidateId, JobPostId).
/// </summary>
public class SavedJob
{
    public string CandidateId { get; set; } = string.Empty;
    public CandidateUser Candidate { get; set; } = null!;

    public int JobPostId { get; set; }
    public JobPost JobPost { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

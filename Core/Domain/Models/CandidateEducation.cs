namespace Domain.Models;

public class CandidateEducation
{
    public int Id { get; set; }
    public string CandidateId { get; set; } = string.Empty;
    public CandidateUser Candidate { get; set; } = null!;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

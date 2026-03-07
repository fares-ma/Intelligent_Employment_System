namespace Services.Abstractions.DTOs.Candidates;

public class ResumeDto
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string FileType { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public string? ExperienceSummary { get; set; }
    public string? EducationSummary { get; set; }
    public string? ActivitiesSummary { get; set; }
    public string? AiGeneratedCvPath { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

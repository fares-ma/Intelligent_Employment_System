namespace Domain.Models;

/// <summary>
/// Candidate's career document — uploaded file + AI-extracted summaries.
/// </summary>
public class Resume
{
    public int Id { get; set; }

    public string CandidateId { get; set; } = string.Empty;
    public CandidateUser Candidate { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFilePath { get; set; } = string.Empty;

    /// <summary>
    /// "pdf" or "docx"
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string? ExperienceSummary { get; set; }

    public string? EducationSummary { get; set; }

    public string? ActivitiesSummary { get; set; }

    public string? AiGeneratedCvPath { get; set; }

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ResumeSkill> ResumeSkills { get; set; } = new List<ResumeSkill>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}

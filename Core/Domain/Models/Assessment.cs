using Domain.Enums;

namespace Domain.Models;

/// <summary>
/// Structured evaluation for a job — contains questions, has a time limit.
/// </summary>
public class Assessment
{
    public int Id { get; set; }

    public int JobPostId { get; set; }
    public JobPost JobPost { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public AssessmentType Type { get; set; }

    public int TimeLimitMinutes { get; set; }

    /// <summary>
    /// Auto-calculated: sum of question points.
    /// </summary>
    public int TotalScore { get; set; }

    public bool IsAiGenerated { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<CandidateAssessment> CandidateAssessments { get; set; } = new List<CandidateAssessment>();
}

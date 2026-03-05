namespace Domain.Models;

/// <summary>
/// Junction: Candidate attempt at an Assessment for a specific JobApplication.
/// Unique constraint on (CandidateId, AssessmentId).
/// </summary>
public class CandidateAssessment
{
    public int Id { get; set; }

    public string CandidateId { get; set; } = string.Empty;
    public CandidateUser Candidate { get; set; } = null!;

    public int AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;

    public int JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    /// <summary>
    /// JSON — submitted answers.
    /// </summary>
    public string? Answers { get; set; }

    public decimal? Score { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public bool IsCompleted { get; set; } = false;
}

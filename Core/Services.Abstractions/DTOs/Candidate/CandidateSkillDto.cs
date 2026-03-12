using Domain.Enums;

namespace Services.Abstractions.DTOs.Candidate;

/// <summary>
/// DTO for displaying candidate skill with proficiency level
/// </summary>
public class CandidateSkillDto
{
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Skill name (e.g., C#, JavaScript, Leadership)
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Proficiency level (Beginner=1, Intermediate=2, Expert=3)
    /// </summary>
    public SkillLevel Level { get; set; } = SkillLevel.Beginner;

    /// <summary>
    /// When this skill was added to profile
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

using Domain.Enums;

namespace Services.Abstractions.DTOs.Candidate;

/// <summary>
/// DTO for adding a new skill to candidate profile
/// </summary>
public class CreateSkillDto
{
    /// <summary>
    /// Skill name (required, 1-100 characters)
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Proficiency level (Beginner=1, Intermediate=2, Expert=3, default Beginner)
    /// </summary>
    public SkillLevel Level { get; set; } = SkillLevel.Beginner;
}

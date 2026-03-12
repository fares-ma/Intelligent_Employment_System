using Domain.Enums;

namespace Services.Abstractions.DTOs.Candidate;

/// <summary>
/// DTO for updating skill proficiency level
/// </summary>
public class UpdateSkillDto
{
    /// <summary>
    /// Proficiency level (Beginner=1, Intermediate=2, Expert=3)
    /// </summary>
    public SkillLevel Level { get; set; }
}

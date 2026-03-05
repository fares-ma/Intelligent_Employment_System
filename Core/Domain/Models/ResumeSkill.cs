namespace Domain.Models;

/// <summary>
/// Junction entity: Resume ↔ Skill (M:N). Composite PK (ResumeId, SkillId).
/// </summary>
public class ResumeSkill
{
    public int ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    /// <summary>
    /// e.g., "Advanced", "Beginner"
    /// </summary>
    public string? Proficiency { get; set; }
}

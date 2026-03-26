namespace Domain.Models;

/// <summary>
/// Junction entity: JobPost ↔ Skill (M:N). Composite PK (JobPostId, SkillId).
/// </summary>
public class JobPostSkill
{
    public int JobPostId { get; set; }
    public JobPost JobPost { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    /// <summary>
    /// Whether this skill is required or nice-to-have.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// The required proficiency level for this skill. Null means any level is acceptable.
    /// </summary>
    public Domain.Enums.SkillLevel? RequiredLevel { get; set; }
}

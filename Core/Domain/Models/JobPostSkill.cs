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
}

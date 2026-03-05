namespace Domain.Models;

/// <summary>
/// Junction entity: CandidateUser ↔ Skill (M:N). Composite PK (CandidateId, SkillId).
/// </summary>
public class CandidateSkill
{
    public string CandidateId { get; set; } = string.Empty;
    public CandidateUser Candidate { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}

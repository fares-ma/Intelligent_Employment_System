using Domain.Enums;

namespace Domain.Models;

/// <summary>
/// Professional capability — can be associated with candidates, jobs, and resumes.
/// </summary>
public class Skill
{
    public int Id { get; set; }

    /// <summary>
    /// Normalized lowercase. Must be unique.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public SkillCategory? Category { get; set; }

    // Navigation properties
    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    public ICollection<JobPostSkill> JobPostSkills { get; set; } = new List<JobPostSkill>();
    public ICollection<ResumeSkill> ResumeSkills { get; set; } = new List<ResumeSkill>();
}

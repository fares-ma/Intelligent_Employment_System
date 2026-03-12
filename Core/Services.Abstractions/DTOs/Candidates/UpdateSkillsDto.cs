namespace Services.Abstractions.DTOs.Candidates;

public class UpdateSkillsDto
{
    /// <summary>
    /// Skill names to set for candidate (replaces existing skills)
    /// </summary>
    public List<string> SkillNames { get; set; } = new();
}

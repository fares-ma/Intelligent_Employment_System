namespace Services.Abstractions.DTOs.Candidates;

public class SkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
}

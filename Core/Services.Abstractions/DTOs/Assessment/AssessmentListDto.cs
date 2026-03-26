namespace Services.Abstractions.DTOs.Assessment;

public class AssessmentListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Type { get; set; }
    public int TimeLimitMinutes { get; set; }
    public int TotalScore { get; set; }
    public bool IsAiGenerated { get; set; }
    public bool IsActive { get; set; }
}

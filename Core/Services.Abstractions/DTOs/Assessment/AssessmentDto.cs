using Domain.Enums;

namespace Services.Abstractions.DTOs.Assessment;

public class AssessmentDto
{
    public int Id { get; set; }
    public int JobPostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssessmentType Type { get; set; }
    public int TimeLimitMinutes { get; set; }
    public int TotalScore { get; set; }
    public bool IsAiGenerated { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string? Options { get; set; }
    public int Points { get; set; }
    public int OrderIndex { get; set; }
}

namespace Services.Abstractions.DTOs.Assessment;

public class AssessmentDetailDto
{
    public int Id { get; set; }
    public int JobPostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Type { get; set; }
    public int TimeLimitMinutes { get; set; }
    public int TotalScore { get; set; }
    public bool IsAiGenerated { get; set; }
    public bool IsActive { get; set; }
    
    public List<AssessmentQuestionDto> Questions { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
}

public class AssessmentQuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Type { get; set; }
    public string? Options { get; set; }
    
    // CorrectAnswer is hidden from candidates in the service mapping during attempt
    public string? CorrectAnswer { get; set; }
    
    public int Points { get; set; }
    public int OrderIndex { get; set; }
}

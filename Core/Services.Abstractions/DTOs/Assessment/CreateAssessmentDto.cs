using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.Assessment;

public class CreateAssessmentDto
{
    [Required]
    public int JobPostId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public AssessmentType Type { get; set; }

    [Required]
    [Range(1, 1440)]
    public int TimeLimitMinutes { get; set; }

    public bool IsAiGenerated { get; set; } = false;

    [Required]
    public List<CreateQuestionDto> Questions { get; set; } = new();
}

public class CreateQuestionDto
{
    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public QuestionType Type { get; set; }

    public string? Options { get; set; } // JSON array for MCQ

    [MaxLength(2000)]
    public string? CorrectAnswer { get; set; }

    [Required]
    [Range(1, 1000)]
    public int Points { get; set; }

    [Required]
    public int OrderIndex { get; set; }
}

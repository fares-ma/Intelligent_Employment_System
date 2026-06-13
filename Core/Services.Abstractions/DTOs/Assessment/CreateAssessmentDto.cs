using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Services.Abstractions.DTOs.Assessment;

public class CreateAssessmentDto
{
    [Required]
    public int JobPostId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public AssessmentType Type { get; set; }

    [Required]
    [Range(1, 300)]
    public int TimeLimitMinutes { get; set; }

    public int? PassingScore { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Instructions { get; set; }

    public List<CreateQuestionDto> Questions { get; set; } = new List<CreateQuestionDto>();
}

public class CreateQuestionDto
{
    [Required]
    public string Text { get; set; } = string.Empty;

    [Required]
    public QuestionType Type { get; set; }

    public string? Options { get; set; }

    public string? CorrectAnswer { get; set; }

    [Required]
    [Range(1, 100)]
    public int Points { get; set; }

    public int OrderIndex { get; set; }
}

using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.Assessment;

public class UpdateAssessmentDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public AssessmentType? Type { get; set; }

    [Range(1, 1440)]
    public int? TimeLimitMinutes { get; set; }

    public bool? IsAiGenerated { get; set; }

    public List<UpdateQuestionDto>? Questions { get; set; }
}

public class UpdateQuestionDto
{
    public int? Id { get; set; } // Provide ID to update existing, null to add new

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public QuestionType Type { get; set; }

    public string? Options { get; set; }

    [MaxLength(2000)]
    public string? CorrectAnswer { get; set; }

    [Required]
    [Range(1, 1000)]
    public int Points { get; set; }

    [Required]
    public int OrderIndex { get; set; }
}

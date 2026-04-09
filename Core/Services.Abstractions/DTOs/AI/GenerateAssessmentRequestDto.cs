using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.AI;

public class GenerateAssessmentRequestDto
{
    [Required(ErrorMessage = "Job description is required.")]
    [MaxLength(5000, ErrorMessage = "Job description cannot exceed 5000 characters.")]
    public required string JobDescription { get; set; }

    [Range(1, 20, ErrorMessage = "Question count must be between 1 and 20.")]
    public int QuestionCount { get; set; } = 5;
}

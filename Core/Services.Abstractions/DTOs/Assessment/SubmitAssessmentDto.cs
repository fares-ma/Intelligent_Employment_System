using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.Assessment;

public class SubmitAssessmentDto
{
    [Required]
    public int JobApplicationId { get; set; }

    [Required]
    public int AssessmentId { get; set; }

    /// <summary>
    /// JSON string of answers. e.g., [{"QuestionId": 1, "Answer": "A"}, ...]
    /// </summary>
    public string? Answers { get; set; }
}

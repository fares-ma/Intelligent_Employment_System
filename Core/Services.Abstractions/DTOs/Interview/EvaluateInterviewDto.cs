using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.Interview;

public class EvaluateInterviewDto
{
    [Required]
    [Range(1, 5)]
    public decimal Rating { get; set; }

    [Required]
    public string FeedbackComments { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.AI;

public class AnalyzeResumeRequestDto
{
    [Required(ErrorMessage = "Resume text is required.")]
    [MaxLength(10000, ErrorMessage = "Resume text cannot exceed 10000 characters.")]
    public required string ResumeText { get; set; }

    [Required(ErrorMessage = "Job description is required.")]
    [MaxLength(5000, ErrorMessage = "Job description cannot exceed 5000 characters.")]
    public required string JobDescription { get; set; }
}

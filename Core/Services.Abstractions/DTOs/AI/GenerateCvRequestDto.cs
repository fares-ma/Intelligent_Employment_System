using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.AI;

public class GenerateCvRequestDto
{
    [Required(ErrorMessage = "Resume text is required.")]
    [MaxLength(10000, ErrorMessage = "Resume text cannot exceed 10000 characters.")]
    public required string ResumeText { get; set; }

    [Required(ErrorMessage = "Candidate name is required.")]
    [MaxLength(100, ErrorMessage = "Candidate name cannot exceed 100 characters.")]
    public required string CandidateName { get; set; }
}

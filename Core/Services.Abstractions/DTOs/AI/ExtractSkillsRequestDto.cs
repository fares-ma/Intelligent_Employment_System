using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.AI;

public class ExtractSkillsRequestDto
{
    [Required(ErrorMessage = "Job description is required.")]
    [MaxLength(5000, ErrorMessage = "Job description cannot exceed 5000 characters.")]
    public required string JobDescription { get; set; }
}

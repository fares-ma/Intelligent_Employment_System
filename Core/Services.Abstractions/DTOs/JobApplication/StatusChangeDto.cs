using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.JobApplication;

public class StatusChangeDto
{
    [Required]
    public int NewStatus { get; set; }
}

public class RatingDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
}

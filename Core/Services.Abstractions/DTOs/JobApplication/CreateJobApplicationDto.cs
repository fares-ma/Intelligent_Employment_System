namespace Services.Abstractions.DTOs.JobApplication;

/// <summary>
/// DTO for creating a new job application
/// </summary>
public class CreateJobApplicationDto
{
    /// <summary>
    /// ID of the job posting to apply for (required)
    /// </summary>
    public int JobPostId { get; set; }

    /// <summary>
    /// Resume ID to attach to this application (required)
    /// </summary>
    public int ResumeId { get; set; }
}

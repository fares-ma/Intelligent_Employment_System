namespace Services.Abstractions.DTOs.AI;

public class AnalyzeResumeRequestDto
{
    public required string ResumeText { get; set; }
    public required string JobDescription { get; set; }
}

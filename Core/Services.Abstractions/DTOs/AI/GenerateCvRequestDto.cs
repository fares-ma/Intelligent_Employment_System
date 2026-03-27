namespace Services.Abstractions.DTOs.AI;

public class GenerateCvRequestDto
{
    public required string ResumeText { get; set; }
    public required string CandidateName { get; set; }
}

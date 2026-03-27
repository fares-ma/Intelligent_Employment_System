namespace Services.Abstractions.DTOs.AI;

public class GenerateAssessmentRequestDto
{
    public required string JobDescription { get; set; }
    public int QuestionCount { get; set; } = 5;
}

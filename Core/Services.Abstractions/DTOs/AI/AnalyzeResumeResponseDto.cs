namespace Services.Abstractions.DTOs.AI;

public class AnalyzeResumeResponseDto
{
    public decimal Score { get; set; }
    public required string Report { get; set; }
}

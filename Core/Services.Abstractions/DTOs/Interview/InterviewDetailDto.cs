namespace Services.Abstractions.DTOs.Interview;

public class InterviewDetailDto : InterviewDto
{
    public string CandidateEmail { get; set; } = string.Empty;
    public string JobPostDescription { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? RecruiterName { get; set; }
}

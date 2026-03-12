namespace Services.Abstractions.DTOs.Candidates;

public class SavedJobDto
{
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string? Location { get; set; }
    public string? JobType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Currency { get; set; }
    public DateTime SavedAt { get; set; }
}

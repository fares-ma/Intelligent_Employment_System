namespace Services.Abstractions.DTOs.JobApplication;

public class ApplicantDto
{
    public int ApplicationId { get; set; }
    
    // Flattened candidate object or a nested class depending on how AutoMapper is set up. 
    // The contract expects "candidate": "{ id, ... }" so we'll use a nested class.
    public ApplicantCandidateDto Candidate { get; set; } = new();
    
    public int Status { get; set; }
    
    public decimal? MatchScore { get; set; }
    
    public string? MatchReport { get; set; }
    
    public int? RecruiterRating { get; set; }
    
    public DateTime AppliedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

public class ApplicantCandidateDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public int? YearsOfExperience { get; set; }
}

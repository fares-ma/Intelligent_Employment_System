namespace Services.Abstractions.DTOs.Auth;

public class LoginResponseDto
{
    public required string Token { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }  // "Admin", "Candidate", "Recruiter"
    public required string UserType { get; set; }  // "CandidateUser" or "Recruiter"
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

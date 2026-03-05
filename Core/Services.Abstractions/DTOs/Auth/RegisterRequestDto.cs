namespace Services.Abstractions.DTOs.Auth;

public class RegisterRequestDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Gender { get; set; }  // "Male", "Female", "Other"
    public required DateTime DateOfBirth { get; set; }
    public required string UserType { get; set; }  // "Candidate" or "Recruiter"
    public int? CompanyId { get; set; }  // Required if UserType == "Recruiter"
}

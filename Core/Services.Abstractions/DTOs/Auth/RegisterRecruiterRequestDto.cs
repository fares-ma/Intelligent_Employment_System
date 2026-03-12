namespace Services.Abstractions.DTOs.Auth;

/// <summary>
/// Register as a recruiter using an invite code (Standard role)
/// </summary>
public class RegisterRecruiterRequestDto
{
    // Personal info
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Gender { get; set; } // "Male", "Female"
    public required DateTime DateOfBirth { get; set; }

    // Invite code
    public required string InviteCode { get; set; }
}

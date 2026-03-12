using Services.Abstractions.DTOs.Auth;

namespace Services.Abstractions;

public interface IAuthService
{
    /// <summary>
    /// Register a new Candidate user
    /// </summary>
    /// <param name="request">Candidate registration details</param>
    /// <returns>LoginResponseDto with generated JWT token</returns>
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Register a new Company and Admin Recruiter
    /// </summary>
    /// <param name="request">Company and admin recruiter details</param>
    /// <returns>LoginResponseDto with admin recruiter JWT token</returns>
    Task<LoginResponseDto> RegisterCompanyAsync(RegisterCompanyRequestDto request);

    /// <summary>
    /// Register a new Recruiter for an existing Company using invite code
    /// </summary>
    /// <param name="request">Recruiter details and valid invite code</param>
    /// <returns>LoginResponseDto with recruiter JWT token</returns>
    Task<LoginResponseDto> RegisterRecruiterAsync(RegisterRecruiterRequestDto request);

    /// <summary>
    /// Authenticate user and return JWT token with role claims
    /// </summary>
    /// <param name="request">Email and password credentials</param>
    /// <returns>LoginResponseDto with token, expiry, user details and role</returns>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Logout user by adding JWT jti to blacklist (invalidates token)
    /// </summary>
    /// <param name="jti">JWT ID (unique identifier) to blacklist</param>
    Task LogoutAsync(string jti);

    /// <summary>
    /// Initiate password reset - sends reset token via email (no information leak)
    /// </summary>
    /// <param name="request">Email address to reset password for</param>
    /// <returns>Success response (same message whether email exists or not)</returns>
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request);

    /// <summary>
    /// Complete password reset with valid reset token
    /// </summary>
    /// <param name="request">Email, reset token (from email), and new password</param>
    Task ResetPasswordAsync(ResetPasswordDto request);

    /// <summary>
    /// Change password for authenticated user (requires current password verification)
    /// </summary>
    /// <param name="userId">Currently authenticated user ID</param>
    /// <param name="request">Current password and new password</param>
    Task ChangePasswordAsync(string userId, ChangePasswordDto request);
}

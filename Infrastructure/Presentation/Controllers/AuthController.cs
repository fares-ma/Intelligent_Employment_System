using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Auth;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new candidate account
    /// </summary>
    /// <response code="201">Candidate registered successfully, token returned</response>
    /// <response code="400">Invalid input or weak password</response>
    /// <response code="409">Email already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("Candidate registration attempt for email: {Email}", request.Email);
        var response = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>
    /// Register a new company with an Admin recruiter (one-step onboarding)
    /// </summary>
    /// <response code="201">Company and Admin recruiter created, token returned</response>
    /// <response code="400">Invalid input, duplicate tax number, or weak password</response>
    /// <response code="409">Email already exists</response>
    [HttpPost("register/company")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoginResponseDto>> RegisterCompany([FromBody] RegisterCompanyRequestDto request)
    {
        _logger.LogInformation("Company registration attempt: {CompanyName} by {Email}", request.CompanyName, request.Email);
        var response = await _authService.RegisterCompanyAsync(request);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>
    /// Register as a Standard recruiter using an invite code
    /// </summary>
    /// <response code="201">Recruiter registered successfully, token returned</response>
    /// <response code="400">Invalid invite code, weak password, or invalid input</response>
    /// <response code="409">Email already exists</response>
    [HttpPost("register/recruiter")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoginResponseDto>> RegisterRecruiter([FromBody] RegisterRecruiterRequestDto request)
    {
        _logger.LogInformation("Recruiter registration attempt (via invite code) for email: {Email}", request.Email);
        var response = await _authService.RegisterRecruiterAsync(request);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>
    /// Authenticate user and receive JWT token
    /// </summary>
    /// <response code="200">Login successful, token returned</response>
    /// <response code="401">Invalid email or password</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Logout current user (invalidate JWT token)
    /// </summary>
    /// <response code="204">Logged out successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        // JWT jti claim — try both the standard registered name and the short name
        var jti = User.FindFirst("jti")?.Value
               ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/jti")?.Value;

        if (!string.IsNullOrEmpty(jti))
        {
            await _authService.LogoutAsync(jti);
            _logger.LogInformation("User {UserId} logged out, jti {Jti} blacklisted",
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value, jti);
        }

        return NoContent();
    }

    /// <summary>
    /// Request password reset (sends reset token via email)
    /// </summary>
    /// <response code="200">Password reset email sent (or would be if email exists)</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        _logger.LogInformation("Forgot password request for email: {Email}", request.Email);
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { message = "If an account exists with this email, a password reset link has been sent." });
    }

    /// <summary>
    /// Reset password with valid reset token
    /// </summary>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Invalid token or weak password</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        _logger.LogInformation("Reset password request for email: {Email}", request.Email);
        await _authService.ResetPasswordAsync(request);
        return Ok(new { message = "Password reset successfully" });
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    /// <response code="200">Password changed successfully</response>
    /// <response code="400">Invalid current password or weak new password</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new Domain.Exceptions.ForbiddenException("User identity not found in token");

        _logger.LogInformation("Change password request for user: {UserId}", userId);
        await _authService.ChangePasswordAsync(userId, request);
        return Ok(new { message = "Password changed successfully" });
    }
}

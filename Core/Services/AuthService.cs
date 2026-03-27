using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Models;
using Domain.Contracts;
using Domain.Exceptions;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Services.Abstractions;
using Services.Abstractions.DTOs.Auth;

namespace Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInviteCodeService _inviteCodeService;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AuthService> _logger;

    // The three application roles
    private static readonly string[] AppRoles = { "Admin", "Recruiter", "Candidate" };

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork unitOfWork,
        IInviteCodeService inviteCodeService,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _inviteCodeService = inviteCodeService;
        _configuration = configuration;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictException("User with this email already exists");

        // Validate user type
        if (request.UserType is not ("Candidate" or "Recruiter"))
            throw new BadRequestException("Invalid user type. Must be 'Candidate' or 'Recruiter'");

        // Parse Gender enum
        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            throw new BadRequestException("Invalid gender. Must be 'Male', 'Female', or 'Other'");

        // Ensure Identity roles exist (idempotent)
        await EnsureRolesExistAsync();

        // Create appropriate user entity based on type
        ApplicationUser user;
        string assignedRole;

        if (request.UserType == "Candidate")
        {
            user = new CandidateUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Gender = gender,
                DateOfBirth = request.DateOfBirth,
                CreatedAt = DateTime.UtcNow
            };
            assignedRole = "Candidate";
        }
        else // Recruiter
        {
            if (!request.CompanyId.HasValue)
                throw new BadRequestException("CompanyId is required for recruiter registration");

            // Check if this is the first recruiter for the company
            var isFirst = await IsFirstRecruiterForCompanyAsync(request.CompanyId.Value);
            assignedRole = isFirst ? "Admin" : "Recruiter";

            user = new Recruiter
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Gender = gender,
                DateOfBirth = request.DateOfBirth,
                CompanyId = request.CompanyId.Value,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Create user with password
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"User creation failed: {errors}");
        }

        // Assign role
        var roleResult = await _userManager.AddToRoleAsync(user, assignedRole);
        if (!roleResult.Succeeded)
        {
            _logger.LogError("Failed to assign role {Role} to user {Email}: {Errors}",
                assignedRole, user.Email,
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {Email} registered as {UserType} with role {Role}",
            user.Email, request.UserType, assignedRole);

        // Generate token directly (don't re-call LoginAsync to avoid double DB hit)
        return BuildLoginResponse(user, assignedRole);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedException("Invalid email or password");

        // Check if account is active
        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated. Contact support.");

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            throw new UnauthorizedException("Invalid email or password");

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "User";

        _logger.LogInformation("User {Email} logged in with role {Role}", user.Email, primaryRole);

        return BuildLoginResponse(user, primaryRole);
    }

    /// <summary>
    /// Register a new company with an Admin recruiter (one-step onboarding)
    /// </summary>
    public async Task<LoginResponseDto> RegisterCompanyAsync(RegisterCompanyRequestDto request)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictException("User with this email already exists");

        // Parse Gender enum
        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            throw new BadRequestException("Invalid gender. Must be 'Male', 'Female', or 'Other'");

        // VALIDATE COMPANY TAX NUMBER UNIQUENESS FIRST (before creating user to prevent orphaned records)
        var existingCompany = await _unitOfWork.Companies.GetByTaxNumberAsync(request.TaxNumber);
        if (existingCompany != null)
            throw new ConflictException("Company with this tax number already exists");

        await EnsureRolesExistAsync();

        // Create Admin recruiter user
        var recruiter = new Recruiter
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Gender = gender,
            DateOfBirth = request.DateOfBirth,
            CompanyId = null, // Will be set after company creation succeeds
            RecruiterRole = UserRole.Admin, // First recruiter is always Admin
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(recruiter, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"User creation failed: {errors}");
        }

        try
        {
            // Create company
            var company = new Company
            {
                Name = request.CompanyName,
                TaxNumber = request.TaxNumber,
                Industry = request.Industry,
                Website = request.Website,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Companies.Create(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Company created: {CompanyName}", request.CompanyName);

            // Update recruiter with company ID
            recruiter.CompanyId = company.Id;
            await _userManager.UpdateAsync(recruiter);

            // Assign Admin role - FATAL if fails
            var roleResult = await _userManager.AddToRoleAsync(recruiter, "Admin");
            if (!roleResult.Succeeded)
            {
                var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to assign Admin role to recruiter {Email}. Errors: {Errors}", recruiter.Email, roleErrors);
                throw new InvalidOperationException($"Failed to assign Admin role: {roleErrors}");
            }

            _logger.LogInformation("Admin recruiter {Email} created for company {CompanyId}", recruiter.Email, company.Id);

            return BuildLoginResponse(recruiter, "Admin");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during company or role setup for recruiter {Email}", recruiter.Email);
            
            // Attempt cleanup
            try
            {
                await _userManager.DeleteAsync(recruiter);
                _logger.LogInformation("Cleaned up recruiter {Email} due to setup failure", recruiter.Email);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Cleanup failed when removing recruiter {Email} after registration failure", recruiter.Email);
                throw new AggregateException(
                    new Exception($"Registration setup failed: {ex.Message}", ex),
                    new Exception($"Cleanup also failed: {cleanupEx.Message}", cleanupEx)
                );
            }
            
            throw;
        }
    }

    /// <summary>
    /// Register as a Standard recruiter using an invite code
    /// </summary>
    public async Task<LoginResponseDto> RegisterRecruiterAsync(RegisterRecruiterRequestDto request)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictException("User with this email already exists");

        // Parse Gender enum
        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            throw new BadRequestException("Invalid gender. Must be 'Male', 'Female', or 'Other'");

        // VALIDATE AND CONSUME INVITE CODE FIRST (before creating user to prevent orphaned records)
        int companyId = await _inviteCodeService.ValidateAndUseAsync(request.InviteCode);

        await EnsureRolesExistAsync();

        // Create Standard recruiter after invite validation succeeds
        var recruiter = new Recruiter
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Gender = gender,
            DateOfBirth = request.DateOfBirth,
            CompanyId = companyId,
            RecruiterRole = UserRole.Standard, // Invited recruiter is Standard
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(recruiter, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"User creation failed: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(recruiter, "Recruiter");
        if (!roleResult.Succeeded)
        {
            _logger.LogError("Failed to assign Recruiter role to user {Email}", recruiter.Email);
        }

        _logger.LogInformation("Standard recruiter {Email} registered for company {CompanyId} via invite code", recruiter.Email, companyId);

        return BuildLoginResponse(recruiter, "Recruiter");
    }

    public Task LogoutAsync(string jti)
    {
        // Add jti to blacklist cache with expiry matching token expiry
        var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryInHours"] ?? "24");
        _memoryCache.Set($"blacklist_{jti}", true, TimeSpan.FromHours(expiryHours));
        _logger.LogInformation("Token jti {Jti} blacklisted for {Hours}h", jti, expiryHours);
        return Task.CompletedTask;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        // Find user by email - no information leak, always return success
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // TODO: Send via IEmailService once implemented in Phase 12 (US10)
            _logger.LogInformation(
                "Password reset token generated for {Email}. Token (dev only): {Token}",
                request.Email, resetToken);
        }
        // Always return — no information leak
    }

    public async Task ResetPasswordAsync(ResetPasswordDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new BadRequestException("Invalid reset request");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Password reset failed: {errors}");
        }
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User", userId);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Password change failed: {errors}");
        }
    }

    public async Task<bool> UserBelongsToCompanyAsync(string userId, int companyId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is Domain.Models.Recruiter recruiter)
        {
            return recruiter.CompanyId == companyId;
        }
        return false;
    }

    // ── Private helpers ──────────────────────────────────────────────

    /// <summary>
    /// Ensures the three application roles exist in AspNetRoles.
    /// Uses RoleManager so it's idempotent and safe to call on every registration.
    /// </summary>
    private async Task EnsureRolesExistAsync()
    {
        foreach (var roleName in AppRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                _logger.LogInformation("Created Identity role: {Role}", roleName);
            }
        }
    }

    /// <summary>
    /// Checks whether any Recruiter entity already exists for the given CompanyId.
    /// Queries UserManager.Users (hits the DB), not claims.
    /// </summary>
    private async Task<bool> IsFirstRecruiterForCompanyAsync(int companyId)
    {
        // UserManager.Users returns IQueryable<ApplicationUser>.
        // Recruiter entities have CompanyId set — filter by type + company.
        var any = _userManager.Users
            .OfType<Recruiter>()
            .Any(r => r.CompanyId == companyId);

        return !any;
    }

    private LoginResponseDto BuildLoginResponse(ApplicationUser user, string role)
    {
        var token = GenerateJwtToken(user, role);

        return new LoginResponseDto
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            UserId = user.Id,
            Email = user.Email!,
            Role = role,
            UserType = user.GetType().Name,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(ApplicationUser user, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured")));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var expiryHours = int.Parse(jwtSettings["ExpiryInHours"] ?? "24");
        var expiresAt = DateTime.UtcNow.AddHours(expiryHours);
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Role, role),
            new("UserType", user.GetType().Name),
            new(JwtRegisteredClaimNames.Jti, jti)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        return (tokenHandler.WriteToken(token), expiresAt);
    }
}

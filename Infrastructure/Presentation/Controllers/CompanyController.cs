using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Company;
using Services.Abstractions.DTOs.InviteCode;
using Domain.Exceptions;

namespace Presentation.Controllers;

/// <summary>
/// Company management endpoints
/// Handles company profile retrieval, updates, and admin transfers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private static readonly string[] AllowedBrandImageExtensions = { ".jpg", ".jpeg", ".png" };
    private const long MaxBrandImageSize = 5 * 1024 * 1024;

    private readonly ICompanyService _companyService;
    private readonly IInviteCodeService _inviteCodeService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService companyService, IInviteCodeService inviteCodeService, ILogger<CompanyController> logger)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _inviteCodeService = inviteCodeService ?? throw new ArgumentNullException(nameof(inviteCodeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public class GenerateInviteCodeRequest
    {
        public int MaxUses { get; set; } = 5;
        public int? ValidDaysFromNow { get; set; } = 30;
    }

    public class GenerateInviteCodeResponse
    {
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Get all companies with active job post counts (paginated)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CompanyProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CompanyProfileDto>>> GetAllCompanies(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var companies = await _companyService.GetAllCompaniesAsync(pageNumber, pageSize);
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all companies");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new company (multipart: fields + optional brand image). Open to any caller; recruiter without a company is linked as admin when authenticated.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CompanyProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyProfileDto>> CreateCompany([FromForm] CreateCompanyForm form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (form.BrandAsset is { Length: > 0 })
        {
            var ext = Path.GetExtension(form.BrandAsset.FileName).ToLowerInvariant();
            if (!AllowedBrandImageExtensions.Contains(ext))
                return BadRequest(new { message = $"Brand image must be one of: {string.Join(", ", AllowedBrandImageExtensions)}" });
            if (form.BrandAsset.Length > MaxBrandImageSize)
                return BadRequest(new { message = $"Brand image cannot exceed {MaxBrandImageSize / (1024 * 1024)}MB" });
        }

        var dto = new CreateCompanyRequestDto
        {
            Name = form.Name,
            Industry = form.Industry,
            Website = form.Website,
            TaxNumber = form.TaxNumber,
            Description = form.Description
        };

        _logger.LogInformation("Create company form submitted (user id: {UserId})", userId ?? "(anonymous)");

        if (form.BrandAsset is { Length: > 0 })
        {
            await using var stream = form.BrandAsset.OpenReadStream();
            var profile = await _companyService.CreateCompanyAsync(userId, dto, stream, form.BrandAsset.FileName);
            return CreatedAtAction(nameof(GetCompanyProfile), new { companyId = profile.Id }, profile);
        }

        var created = await _companyService.CreateCompanyAsync(userId, dto, null, null);
        return CreatedAtAction(nameof(GetCompanyProfile), new { companyId = created.Id }, created);
    }

    /// <summary>
    /// Get company profile information
    /// </summary>
    /// <param name="companyId">Company identifier</param>
    /// <returns>Company profile details</returns>
    [HttpGet("{companyId}")]
    public async Task<ActionResult<CompanyProfileDto>> GetCompanyProfile(string companyId)
    {
        _logger.LogInformation("Getting company profile for ID: {CompanyId}", companyId);

        try
        {
            var profile = await _companyService.GetCompanyProfileAsync(companyId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company profile for ID: {CompanyId}", companyId);
            throw;
        }
    }

    /// <summary>
    /// Update company information
    /// Only company admin can update
    /// </summary>
    /// <param name="companyId">Company identifier</param>
    /// <param name="request">Update request with new company information</param>
    /// <returns>Updated company profile</returns>
    [HttpPut("{companyId}")]
    public async Task<ActionResult<CompanyProfileDto>> UpdateCompany(string companyId, [FromBody] UpdateCompanyDto request)
    {
        _logger.LogInformation("Updating company {CompanyId} for user {UserId}", companyId, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _companyService.UpdateCompanyAsync(companyId, userId, request);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company {CompanyId}", companyId);
            throw;
        }
    }

    /// <summary>
    /// Transfer admin role to another recruiter
    /// Only current admin can initiate
    /// </summary>
    /// <param name="companyId">Company identifier</param>
    /// <param name="request">Transfer request with new admin recruiter ID</param>
    /// <returns>Confirmation of admin transfer</returns>
    [HttpPost("{companyId}/transfer-admin")]
    public async Task<ActionResult<AdminTransferResponseDto>> TransferAdmin(string companyId, [FromBody] AdminTransferDto request)
    {
        _logger.LogInformation("Admin transfer requested for company {CompanyId} by user {UserId}", companyId, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _companyService.TransferAdminAsync(companyId, userId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring admin for company {CompanyId}", companyId);
            throw;
        }
    }

    /// <summary>
    /// Update company logo
    /// </summary>
    [HttpPut("{companyId}/logo")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> UpdateLogo(string companyId, IFormFile file)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Validate file
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Image file is required and cannot be empty" });

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Only .jpg, .jpeg, .png images are allowed" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Image size cannot exceed 5MB" });

        _logger.LogInformation("Company logo upload for company {CompanyId}: {FileName}", companyId, file.FileName);

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var picturePath = await _companyService.UpdateLogoAsync(companyId, file.FileName, stream);
                return Ok(new { picturePath });
            }
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get count of active invite codes for the company
    /// Only company admin can view
    /// </summary>
    /// <param name="companyId">Company identifier</param>
    /// <returns>Count of active invitation codes</returns>
    [HttpGet("{companyId}/active-invitations-count")]
    public async Task<ActionResult<int>> GetActiveInvitationsCount(string companyId)
    {
        _logger.LogInformation("Getting active invitations count for company {CompanyId}", companyId);

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var count = await _companyService.GetActiveInvitationsCountAsync(companyId, userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active invitations count for company {CompanyId}", companyId);
            throw;
        }
    }

    /// <summary>
    /// Generate a new recruiter invite code for the company
    /// Only company admin can generate invite codes
    /// </summary>
    [HttpPost("{companyId}/invite-codes")]
    [ProducesResponseType(typeof(GenerateInviteCodeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GenerateInviteCodeResponse>> GenerateInviteCode(string companyId, [FromBody] GenerateInviteCodeRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!int.TryParse(companyId, out var parsedCompanyId))
        {
            return BadRequest(new { message = "Invalid company id." });
        }

        // Reuse existing admin authorization rules from company service.
        await _companyService.GetActiveInvitationsCountAsync(companyId, userId);

        var maxUses = request?.MaxUses ?? 5;
        var validDays = request?.ValidDaysFromNow;
        var code = await _inviteCodeService.GenerateAsync(parsedCompanyId, userId, maxUses, validDays);

        return CreatedAtAction(nameof(GetInviteCodes), new { companyId }, new GenerateInviteCodeResponse { Code = code });
    }

    /// <summary>
    /// List active recruiter invite codes for the company
    /// Only company admin can list invite codes
    /// </summary>
    [HttpGet("{companyId}/invite-codes")]
    [ProducesResponseType(typeof(IEnumerable<InviteCodeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<InviteCodeDto>>> GetInviteCodes(string companyId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!int.TryParse(companyId, out var parsedCompanyId))
        {
            return BadRequest(new { message = "Invalid company id." });
        }

        // Reuse existing admin authorization rules from company service.
        await _companyService.GetActiveInvitationsCountAsync(companyId, userId);

        var codes = await _inviteCodeService.GetActiveCodesAsync(parsedCompanyId);
        return Ok(codes);
    }

    /// <summary>
    /// Revoke an active recruiter invite code
    /// Only company admin can revoke invite codes
    /// </summary>
    [HttpDelete("{companyId}/invite-codes/{codeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeInviteCode(string companyId, int codeId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Reuse existing admin authorization rules from company service.
        await _companyService.GetActiveInvitationsCountAsync(companyId, userId);

        await _inviteCodeService.RevokeAsync(codeId);
        return NoContent();
    }
}


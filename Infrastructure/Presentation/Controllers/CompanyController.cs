using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Company;

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
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
}


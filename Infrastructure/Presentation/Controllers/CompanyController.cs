using Microsoft.AspNetCore.Authorization;
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
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        _logger.LogInformation("Updating company {CompanyId} for user {UserId}", companyId, User.FindFirst("sub")?.Value);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst("sub")?.Value;
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
        _logger.LogInformation("Admin transfer requested for company {CompanyId} by user {UserId}", companyId, User.FindFirst("sub")?.Value);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst("sub")?.Value;
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
            var userId = User.FindFirst("sub")?.Value;
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


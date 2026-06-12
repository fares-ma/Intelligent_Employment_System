using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Dashboard;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardsController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IAuthService _authService; // To map UserId to CompanyId ideally

    public DashboardsController(IDashboardService dashboardService, IAuthService authService)
    {
        _dashboardService = dashboardService;
        _authService = authService;
    }

    [HttpGet("candidate")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(CandidateDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CandidateDashboardDto>> GetCandidateDashboard()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || string.IsNullOrEmpty(claim.Value))
            return Unauthorized("User identifier not found.");

        var candidateId = claim.Value;
        var result = await _dashboardService.GetCandidateDashboardAsync(candidateId);
        return Ok(result);
    }

    [HttpGet("company/{companyId}")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(CompanyDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyDashboardDto>> GetCompanyDashboard(int companyId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            return Unauthorized("User identifier not found.");

        var companyIdClaim = User.FindFirst("companyId");
        if (companyIdClaim != null && int.TryParse(companyIdClaim.Value, out var claimCompanyId))
        {
            if (companyId == 0)
            {
                companyId = claimCompanyId;
            }
            else if (companyId != claimCompanyId)
            {
                return Forbid();
            }
        }
        else if (companyId == 0)
        {
            return BadRequest("Company ID is required.");
        }

        if (!User.IsInRole("Admin") && companyIdClaim == null)
        {
            var belongsToCompany = await _authService.UserBelongsToCompanyAsync(userIdClaim.Value, companyId);
            if (!belongsToCompany) return Forbid();
        }

        var result = await _dashboardService.GetCompanyDashboardAsync(companyId);
        return Ok(result);
    }
}

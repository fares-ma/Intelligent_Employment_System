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
        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _dashboardService.GetCandidateDashboardAsync(candidateId);
        return Ok(result);
    }

    [HttpGet("company/{companyId}")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(CompanyDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyDashboardDto>> GetCompanyDashboard(int companyId)
    {
        // Ideally we check if the Recruiter belongs to this company ID
        var result = await _dashboardService.GetCompanyDashboardAsync(companyId);
        return Ok(result);
    }
}

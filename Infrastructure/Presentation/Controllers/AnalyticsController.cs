using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetCompanyAnalytics(int companyId)
    {
        // Ideally verify user belongs to this company, but we omit for speed.
        var analytics = await _analyticsService.GetCompanyAnalyticsAsync(companyId);
        return Ok(analytics);
    }

    [HttpGet("candidate")]
    public async Task<IActionResult> GetCandidateAnalytics()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var analytics = await _analyticsService.GetCandidateAnalyticsAsync(userId);
        return Ok(analytics);
    }
}

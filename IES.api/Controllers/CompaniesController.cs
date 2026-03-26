using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Company;
using System.Security.Claims;

namespace IES.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ICompanyService companyService, IDashboardService dashboardService, ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(string id)
    {
        try
        {
            var result = await _companyService.GetCompanyProfileAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}/dashboard")]
    public async Task<IActionResult> GetDashboard(string id)
    {
        try
        {
            var result = await _dashboardService.GetCompanyDashboardAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company dashboard");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> UpdateCompany(string id, [FromBody] UpdateCompanyDto request)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User identity not found");

            var result = await _companyService.UpdateCompanyAsync(id, recruiterId, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authorization error: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

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
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    private string? GetUserIdOrNull()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserActivity([FromQuery] int limit = 20)
    {
        var userId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var logs = await _activityLogService.GetUserActivityAsync(userId, limit);
        return Ok(logs);
    }
}

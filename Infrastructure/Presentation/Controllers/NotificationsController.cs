using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Notification;
using Shared.Pagination;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private string? GetUserIdOrNull()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim?.Value;
    }

    private ActionResult UnauthorizedIfNoUser()
    {
        var userId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found.");
        return Ok(); // Placeholder - caller should use userId
    }

    [HttpGet]
    [ProducesResponseType(typeof(Shared.Pagination.PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Shared.Pagination.PagedResult<NotificationDto>>> GetMyNotifications(
        [FromQuery] PaginationParams pagination,
        [FromQuery] bool? isRead)
    {
        var userId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found.");
        
        var result = await _notificationService.GetUserNotificationsAsync(userId, pagination, isRead);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount()
    {
        var userId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found.");
        
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new UnreadCountDto { Count = count });
    }

    [HttpPatch("{id}/mark-read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var userId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found.");
        
        await _notificationService.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found.");
        
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}

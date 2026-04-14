using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Message;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private string? GetUserIdOrNull()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var senderId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(senderId)) return Unauthorized();

        var message = await _messageService.SendMessageAsync(senderId, request);
        return Ok(message);
    }

    [HttpGet("conversation/{userId}")]
    public async Task<IActionResult> GetConversation(string userId)
    {
        var currentUserId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var thread = await _messageService.GetConversationAsync(currentUserId, userId);
        return Ok(thread);
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var currentUserId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var conversations = await _messageService.GetUserConversationsAsync(currentUserId);
        return Ok(conversations);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUserId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var count = await _messageService.GetUnreadCountAsync(currentUserId);
        return Ok(new { Count = count });
    }

    [HttpPatch("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var currentUserId = GetUserIdOrNull();
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        await _messageService.MarkAsReadAsync(id, currentUserId);
        return NoContent();
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Presentation.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private static readonly Dictionary<string, HashSet<string>> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("nameid")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            lock (_userConnections)
            {
                if (!_userConnections.ContainsKey(userId))
                    _userConnections[userId] = new HashSet<string>();
                _userConnections[userId].Add(Context.ConnectionId);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("nameid")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            lock (_userConnections)
            {
                if (_userConnections.ContainsKey(userId))
                    _userConnections[userId].Remove(Context.ConnectionId);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendNotification(string userId, string type, string title, string message, string? targetUrl)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", new
        {
            type,
            title,
            message,
            targetUrl,
            createdAt = DateTime.UtcNow
        });
    }

    public async Task UpdateUnreadCount(string userId, int count)
    {
        await Clients.User(userId).SendAsync("UnreadCountUpdated", count);
    }
}

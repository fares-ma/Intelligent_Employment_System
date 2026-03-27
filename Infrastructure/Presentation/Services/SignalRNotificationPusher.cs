using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;
using Services.Abstractions;
using Services.Abstractions.DTOs.Notification;

namespace Infrastructure.Presentation.Services;

public class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPusher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushNotificationAsync(string userId, NotificationDto notificationDto)
    {
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notificationDto);
    }

    public async Task UpdateUnreadCountAsync(string userId, int count)
    {
        await _hubContext.Clients.Group(userId).SendAsync("UpdateUnreadCount", count);
    }
}

using Services.Abstractions.DTOs.Notification;

namespace Services.Abstractions;

public interface INotificationPusher
{
    Task PushNotificationAsync(string userId, NotificationDto notificationDto);
    Task UpdateUnreadCountAsync(string userId, int count);
}

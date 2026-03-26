using Services.Abstractions.DTOs.Notification;

namespace Services.Abstractions;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto request);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, int pageNumber = 1, int pageSize = 20, bool? isRead = null);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task SendNotificationAsync(string userId, string type, string title, string message, string? targetUrl = null);
}

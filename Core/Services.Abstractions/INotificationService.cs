using Services.Abstractions.DTOs.Notification;
using Shared.Pagination;

namespace Services.Abstractions;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string title, string message, string type, string? targetUrl = null);
    Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(string userId, PaginationParams pagination, bool? isRead = null);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
}

using Domain.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Presentation.Hubs;
using Services.Abstractions;
using Services.Abstractions.DTOs.Notification;
using Shared.Pagination;

namespace Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto request)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            TargetUrl = request.TargetUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Notifications.Create(notification);
        await _unitOfWork.SaveChangesAsync();

        await SendNotificationAsync(request.UserId, request.Type, request.Title, request.Message, request.TargetUrl);

        return MapToDto(notification);
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, int pageNumber = 1, int pageSize = 20, bool? isRead = null)
    {
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _unitOfWork.Notifications.GetByUserAsync(userId, pagination, isRead);

        return result.Items.Select(MapToDto);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        await _unitOfWork.Notifications.MarkReadAsync(notificationId);
        await _unitOfWork.SaveChangesAsync();

        var count = await GetUnreadCountAsync(userId);
        await _hubContext.Clients.User(userId).SendAsync("UnreadCountUpdated", count);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _unitOfWork.Notifications.MarkAllReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        await _hubContext.Clients.User(userId).SendAsync("UnreadCountUpdated", 0);
    }

    public async Task SendNotificationAsync(string userId, string type, string title, string message, string? targetUrl = null)
    {
        _logger.LogInformation("Sending notification to user {UserId}: {Title}", userId, title);

        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
        {
            type,
            title,
            message,
            targetUrl,
            createdAt = DateTime.UtcNow
        });

        var count = await GetUnreadCountAsync(userId);
        await _hubContext.Clients.User(userId).SendAsync("UnreadCountUpdated", count);

        try
        {
            await _emailService.SendNotificationEmailAsync(userId, title, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email notification to {UserId}", userId);
        }
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            TargetUrl = notification.TargetUrl,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt
        };
    }
}

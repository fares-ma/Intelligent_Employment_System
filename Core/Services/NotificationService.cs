using AutoMapper;
using Domain.Contracts;
using Domain.Exceptions;
using Domain.Models;
using Services.Abstractions;
using Services.Abstractions.DTOs.Notification;
using Shared.Pagination;

namespace Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPusher _notificationPusher;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public NotificationService(
        IUnitOfWork unitOfWork,
        INotificationPusher notificationPusher,
        IEmailService emailService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _notificationPusher = notificationPusher;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task SendNotificationAsync(string userId, string title, string message, string type, string? targetUrl = null)
    {
        var user = await _unitOfWork.Candidates.GetByIdAsync(userId);
        if (user == null) return;

        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            TargetUrl = targetUrl
        };

        _unitOfWork.Notifications.Create(notification);
        await _unitOfWork.SaveChangesAsync();

        var notificationDto = _mapper.Map<NotificationDto>(notification);

        // Send via INotificationPusher
        await _notificationPusher.PushNotificationAsync(userId, notificationDto);

        // Simple Email Notification
        // Need to load User to get email ideally, assuming we get it from Identity
        // For simplicity we just trace it for now via EmailService Mock
        await _emailService.SendEmailAsync(userId, $"IES Notification: {title}", message);
    }

    public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(string userId, PaginationParams pagination, bool? isRead = null)
    {
        var pagedResult = await _unitOfWork.Notifications.GetByUserAsync(userId, pagination, isRead);
        
        var dtoList = _mapper.Map<IEnumerable<NotificationDto>>(pagedResult.Items).ToList();
        
        return new PagedResult<NotificationDto>(dtoList, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
            throw new NotFoundException("Notification not found");

        await _unitOfWork.Notifications.MarkReadAsync(notificationId);
        await _unitOfWork.SaveChangesAsync();

        // Push new unread count
        var count = await GetUnreadCountAsync(userId);
        await _notificationPusher.UpdateUnreadCountAsync(userId, count);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _unitOfWork.Notifications.MarkAllReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        await _notificationPusher.UpdateUnreadCountAsync(userId, 0);
    }
}

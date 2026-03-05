using Domain.Models;
using Shared.Pagination;

namespace Domain.Contracts;

public interface INotificationRepository : IRepositoryBase<Notification>
{
    Task<PagedResult<Notification>> GetByUserAsync(string userId, PaginationParams pagination, bool? isRead = null, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkReadAsync(int notificationId, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(string userId, CancellationToken cancellationToken = default);
}

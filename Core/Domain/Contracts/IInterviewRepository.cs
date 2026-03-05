using Domain.Models;
using Shared.Pagination;

namespace Domain.Contracts;

public interface IInterviewRepository : IRepositoryBase<Interview>
{
    Task<IEnumerable<Interview>> GetByApplicationAsync(int applicationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Interview>> GetUpcomingAsync(string userId, CancellationToken cancellationToken = default);
    Task<PagedResult<Interview>> GetByUserAsync(string userId, PaginationParams pagination, CancellationToken cancellationToken = default);
}

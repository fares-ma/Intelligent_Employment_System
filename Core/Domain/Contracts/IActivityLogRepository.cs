using Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IActivityLogRepository : IRepositoryBase<ActivityLog>
    {
        Task<IEnumerable<ActivityLog>> GetUserActivityAsync(string userId, int limit, CancellationToken cancellationToken = default);
    }
}

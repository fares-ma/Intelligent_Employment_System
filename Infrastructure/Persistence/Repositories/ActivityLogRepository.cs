using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class ActivityLogRepository : RepositoryBase<ActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ActivityLog>> GetUserActivityAsync(string userId, int limit, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }
    }
}

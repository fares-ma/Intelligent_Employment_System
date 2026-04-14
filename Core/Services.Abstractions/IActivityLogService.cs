using Services.Abstractions.DTOs.ActivityLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IActivityLogService
    {
        Task LogActionAsync(string userId, string actionType, string? details = null);
        Task<IEnumerable<ActivityLogDto>> GetUserActivityAsync(string userId, int limit = 20);
    }
}

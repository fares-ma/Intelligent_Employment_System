using Domain.Contracts;
using Domain.Models;
using Services.Abstractions;
using Services.Abstractions.DTOs.ActivityLog;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActivityLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogActionAsync(string userId, string actionType, string? details = null)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                ActionType = actionType,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.ActivityLogs.Create(log);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ActivityLogDto>> GetUserActivityAsync(string userId, int limit = 20)
        {
            var logs = await _unitOfWork.ActivityLogs.GetUserActivityAsync(userId, limit);
            return logs.Select(a => new ActivityLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                ActionType = a.ActionType,
                Details = a.Details,
                CreatedAt = a.CreatedAt
            });
        }
    }
}

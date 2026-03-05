using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Shared.Pagination;

namespace Persistence.Repositories;

public class InterviewRepository : RepositoryBase<Interview>, IInterviewRepository
{
    public InterviewRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Interview>> GetByApplicationAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.JobApplicationId == applicationId)
            .AsNoTracking()
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Interview>> GetUpcomingAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.JobApplication)
                .ThenInclude(ja => ja.JobPost)
                    .ThenInclude(j => j.Company)
            .Where(i => i.ScheduledAt > DateTime.UtcNow &&
                        i.Status != Domain.Enums.InterviewStatus.Cancelled)
            .Where(i => i.JobApplication.CandidateId == userId ||
                        i.JobApplication.JobPost.CreatedByRecruiterId == userId)
            .AsNoTracking()
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Interview>> GetUpcomingPagedAsync(string userId, PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(i => i.JobApplication)
                .ThenInclude(ja => ja.JobPost)
                    .ThenInclude(j => j.Company)
            .Where(i => i.ScheduledAt > DateTime.UtcNow &&
                        i.Status != Domain.Enums.InterviewStatus.Cancelled)
            .Where(i => i.JobApplication.CandidateId == userId ||
                        i.JobApplication.JobPost.CreatedByRecruiterId == userId)
            .AsNoTracking()
            .OrderBy(i => i.ScheduledAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Interview>(items, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
    }

    public async Task<PagedResult<Interview>> GetByUserAsync(string userId, PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(i => i.JobApplication)
                .ThenInclude(ja => ja.JobPost)
                    .ThenInclude(j => j.Company)
            .Where(i => i.JobApplication.CandidateId == userId ||
                        i.JobApplication.JobPost.CreatedByRecruiterId == userId)
            .AsNoTracking()
            .OrderByDescending(i => i.ScheduledAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Interview>(items, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
    }
}

using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Shared.Pagination;

namespace Persistence.Repositories;

public class SavedJobRepository : RepositoryBase<SavedJob>, ISavedJobRepository
{
    public SavedJobRepository(AppDbContext context) : base(context) { }

    public async Task<SavedJob?> GetAsync(string candidateId, int jobPostId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(sj => sj.CandidateId == candidateId && sj.JobPostId == jobPostId, cancellationToken);
    }

    public async Task<IEnumerable<SavedJob>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(sj => sj.CandidateId == candidateId)
            .Include(sj => sj.JobPost)
                .ThenInclude(j => j.Company)
            .Include(sj => sj.JobPost)
                .ThenInclude(j => j.JobPostSkills)
                    .ThenInclude(jps => jps.Skill)
            .AsNoTracking()
            .OrderByDescending(sj => sj.SavedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<SavedJob>> GetByCandidatePagedAsync(string candidateId, PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(sj => sj.CandidateId == candidateId)
            .Include(sj => sj.JobPost)
                .ThenInclude(j => j.Company)
            .Include(sj => sj.JobPost)
                .ThenInclude(j => j.JobPostSkills)
                    .ThenInclude(jps => jps.Skill)
            .AsNoTracking()
            .OrderByDescending(sj => sj.SavedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SavedJob>(items, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
    }
}

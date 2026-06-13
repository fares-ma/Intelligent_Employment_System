using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Shared.Pagination;

namespace Persistence.Repositories;

public class JobPostRepository : RepositoryBase<JobPost>, IJobPostRepository
{
    public JobPostRepository(AppDbContext context) : base(context) { }

    public async Task<JobPost?> GetByIdWithSkillsAsync(int jobPostId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(j => j.JobPostSkills)
                .ThenInclude(jps => jps.Skill)
            .Include(j => j.Company)
            .Include(j => j.JobApplications)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobPostId, cancellationToken);
    }

    public async Task<PagedResult<JobPost>> GetPublishedJobsAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(j => j.IsPublished && j.IsActive && !j.IsDeleted && j.Status == "ACTIVE" && j.ExpiryDate > DateTime.UtcNow)
            .Include(j => j.JobPostSkills)
                .ThenInclude(jps => jps.Skill)
            .Include(j => j.Company)
            .Include(j => j.JobApplications)
            .AsNoTracking()
            .OrderByDescending(j => j.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<JobPost>(items, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
    }

    public async Task<IEnumerable<JobPost>> GetSimilarJobsAsync(int jobPostId, int count, CancellationToken cancellationToken = default)
    {
        var job = await _dbSet
            .Include(j => j.JobPostSkills)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobPostId, cancellationToken);

        if (job is null) return Enumerable.Empty<JobPost>();

        var skillIds = job.JobPostSkills.Select(jps => jps.SkillId).ToList();

        return await _dbSet
            .Where(j => j.Id != jobPostId && j.IsPublished && j.IsActive && !j.IsDeleted && j.Status == "ACTIVE" && j.ExpiryDate > DateTime.UtcNow)
            .Where(j => j.JobPostSkills.Any(jps => skillIds.Contains(jps.SkillId)))
            .Include(j => j.JobPostSkills)
                .ThenInclude(jps => jps.Skill)
            .Include(j => j.Company)
            .Include(j => j.JobApplications)
            .AsNoTracking()
            .OrderByDescending(j => j.JobPostSkills.Count(jps => skillIds.Contains(jps.SkillId)))
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<JobPost>> GetCompanyJobsAsync(int companyId, PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(j => j.CompanyId == companyId && !j.IsDeleted)
            .Include(j => j.JobPostSkills)
                .ThenInclude(jps => jps.Skill)
            .Include(j => j.Company)
            .Include(j => j.JobApplications)
            .AsNoTracking()
            .OrderByDescending(j => j.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<JobPost>(items, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
    }
}

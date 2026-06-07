using Domain.Contracts;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Shared.Pagination;

namespace Persistence.Repositories;

public class JobApplicationRepository : RepositoryBase<JobApplication>, IJobApplicationRepository
{
    public JobApplicationRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExistsAsync(string candidateId, int jobPostId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            ja => ja.CandidateId == candidateId && ja.JobPostId == jobPostId,
            cancellationToken);
    }

    public async Task<JobApplication?> GetByTalentXCorrelationAsync(
        int talentXCandidateId,
        string candidateId,
        int jobPostId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                ja => (ja.TalentXSentCandidateId == talentXCandidateId || ja.CandidateId == candidateId) 
                      && ja.JobPostId == jobPostId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<JobApplication>> GetStuckInProcessingAsync(
        TimeSpan olderThan,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(olderThan);

        return await _dbSet
            .Where(ja =>
                ja.AiScoringStatus == AiScoringStatus.Processing
                && ja.MatchScore == null
                && ja.AiScoringRequestedAt != null
                && ja.AiScoringRequestedAt < cutoff)
            .OrderBy(ja => ja.AiScoringRequestedAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<JobApplication>> GetByCandidateAsync(string candidateId, PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(ja => ja.CandidateId == candidateId)
            .Include(ja => ja.JobPost)
                .ThenInclude(j => j.Company)
            .AsNoTracking()
            .OrderByDescending(ja => ja.AppliedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<JobApplication>(items, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
    }

    public async Task<PagedResult<JobApplication>> GetByJobPostAsync(int jobPostId, PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(ja => ja.JobPostId == jobPostId)
            .Include(ja => ja.Candidate)
            .Include(ja => ja.Resume)
            .AsNoTracking()
            .OrderByDescending(ja => ja.AppliedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<JobApplication>(items, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
    }
}

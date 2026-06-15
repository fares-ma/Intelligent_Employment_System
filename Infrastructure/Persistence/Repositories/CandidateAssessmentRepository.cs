using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Pagination;
using Persistence.Data;

namespace Persistence.Repositories;

public class CandidateAssessmentRepository : RepositoryBase<CandidateAssessment>, ICandidateAssessmentRepository
{
    public CandidateAssessmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<CandidateAssessment?> GetByCandidateAndAssessmentAsync(string candidateId, int assessmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CandidateAssessment>()
            .Include(ca => ca.Assessment)
                .ThenInclude(a => a.Questions)
            .FirstOrDefaultAsync(ca => ca.CandidateId == candidateId && ca.AssessmentId == assessmentId, cancellationToken);
    }

    public async Task<IEnumerable<CandidateAssessment>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CandidateAssessment>()
            .Include(ca => ca.Assessment)
            .Where(ca => ca.CandidateId == candidateId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<CandidateAssessment>> GetPagedByAssessmentAsync(int assessmentId, PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<CandidateAssessment>()
            .Include(ca => ca.Candidate)
            .Where(ca => ca.AssessmentId == assessmentId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(ca => ca.SubmittedAt)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CandidateAssessment>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize
        };
    }
}

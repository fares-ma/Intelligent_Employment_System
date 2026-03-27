using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
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
            .FirstOrDefaultAsync(ca => ca.CandidateId == candidateId && ca.AssessmentId == assessmentId, cancellationToken);
    }

    public async Task<IEnumerable<CandidateAssessment>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CandidateAssessment>()
            .Include(ca => ca.Assessment)
            .Where(ca => ca.CandidateId == candidateId)
            .ToListAsync(cancellationToken);
    }
}

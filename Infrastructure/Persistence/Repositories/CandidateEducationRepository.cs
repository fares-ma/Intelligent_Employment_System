using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class CandidateEducationRepository : RepositoryBase<CandidateEducation>, ICandidateEducationRepository
{
    public CandidateEducationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<CandidateEducation>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.CandidateId == candidateId)
            .AsNoTracking()
            .OrderByDescending(e => e.GraduationYear)
            .ToListAsync(cancellationToken);
    }
}

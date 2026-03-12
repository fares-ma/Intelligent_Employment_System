using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class CandidateExperienceRepository : RepositoryBase<CandidateExperience>, ICandidateExperienceRepository
{
    public CandidateExperienceRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<CandidateExperience>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.CandidateId == candidateId)
            .AsNoTracking()
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }
}

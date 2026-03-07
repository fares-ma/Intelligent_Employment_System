using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class CandidateRepository : RepositoryBase<CandidateUser>, ICandidateRepository
{
    public CandidateRepository(AppDbContext context) : base(context) { }

    public async Task<CandidateUser?> GetWithSkillsAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(c => c.Id == candidateId, cancellationToken);
    }

    public async Task<CandidateUser?> GetWithResumesAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Resumes)
            .FirstOrDefaultAsync(c => c.Id == candidateId, cancellationToken);
    }
}

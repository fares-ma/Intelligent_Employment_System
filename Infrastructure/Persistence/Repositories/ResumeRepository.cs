using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class ResumeRepository : RepositoryBase<Resume>, IResumeRepository
{
    public ResumeRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Resume>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.CandidateId == candidateId)
            .Include(r => r.ResumeSkills)
                .ThenInclude(rs => rs.Skill)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt) // Models dont have UploadedAt yet
            .ToListAsync(cancellationToken);
    }

    public async Task<Resume?> GetDefaultByCandidateAsync(string candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.CandidateId == candidateId && r.IsDefault)
            .Include(r => r.ResumeSkills)
                .ThenInclude(rs => rs.Skill)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }
}

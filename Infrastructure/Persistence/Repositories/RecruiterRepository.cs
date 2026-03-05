using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class RecruiterRepository : RepositoryBase<Recruiter>, IRecruiterRepository
{
    public RecruiterRepository(AppDbContext context) : base(context) { }

    public async Task<Recruiter?> GetWithCompanyAsync(string recruiterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == recruiterId, cancellationToken);
    }

    public async Task<IEnumerable<Recruiter>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.CompanyId == companyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class CompanyInviteCodeRepository : RepositoryBase<CompanyInviteCode>, ICompanyInviteCodeRepository
{
    public CompanyInviteCodeRepository(AppDbContext context) : base(context) { }

    public async Task<CompanyInviteCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<CompanyInviteCode>> GetActiveByCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CompanyId == companyId && c.IsActive && c.ExpiresAt > DateTime.UtcNow)
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

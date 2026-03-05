using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
{
    public CompanyRepository(AppDbContext context) : base(context) { }

    public async Task<Company?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TaxNumber == taxNumber, cancellationToken);
    }

    public async Task<bool> TaxNumberExistsAsync(string taxNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.TaxNumber == taxNumber, cancellationToken);
    }
}

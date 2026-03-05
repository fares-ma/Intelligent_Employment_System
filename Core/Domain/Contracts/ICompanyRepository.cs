using Domain.Models;

namespace Domain.Contracts;

public interface ICompanyRepository : IRepositoryBase<Company>
{
    Task<Company?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default);
    Task<bool> TaxNumberExistsAsync(string taxNumber, CancellationToken cancellationToken = default);
}

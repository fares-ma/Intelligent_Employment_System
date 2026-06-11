using Domain.Models;

namespace Domain.Contracts;

public interface ICompanyRepository : IRepositoryBase<Company>
{
    Task<Company?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default);
    Task<bool> TaxNumberExistsAsync(string taxNumber, CancellationToken cancellationToken = default);

    /// <param name="tracking">When false, uses no-tracking query (read scenarios).</param>
    Task<Company?> GetByIdWithIncludesAsync(int id, bool tracking = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all companies with Recruiters and JobPosts included
    /// </summary>
    Task<IEnumerable<Company>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default);
}

using Domain.Models;

namespace Domain.Contracts;

public interface ICompanyInviteCodeRepository : IRepositoryBase<CompanyInviteCode>
{
    Task<CompanyInviteCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<CompanyInviteCode>> GetActiveByCompanyAsync(int companyId, CancellationToken cancellationToken = default);
}

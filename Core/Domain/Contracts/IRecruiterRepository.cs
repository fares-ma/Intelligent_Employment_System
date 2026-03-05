using Domain.Models;

namespace Domain.Contracts;

public interface IRecruiterRepository : IRepositoryBase<Recruiter>
{
    Task<Recruiter?> GetWithCompanyAsync(string recruiterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recruiter>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken = default);
}

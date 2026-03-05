using Domain.Models;

namespace Domain.Contracts;

public interface IResumeRepository : IRepositoryBase<Resume>
{
    Task<IEnumerable<Resume>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default);
    Task<Resume?> GetDefaultByCandidateAsync(string candidateId, CancellationToken cancellationToken = default);
}

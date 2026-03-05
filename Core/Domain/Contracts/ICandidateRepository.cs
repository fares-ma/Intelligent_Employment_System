using Domain.Models;

namespace Domain.Contracts;

public interface ICandidateRepository : IRepositoryBase<CandidateUser>
{
    Task<CandidateUser?> GetWithSkillsAsync(string candidateId, CancellationToken cancellationToken = default);
    Task<CandidateUser?> GetWithResumesAsync(string candidateId, CancellationToken cancellationToken = default);
}

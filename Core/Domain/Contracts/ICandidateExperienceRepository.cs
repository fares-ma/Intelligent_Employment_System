using Domain.Models;

namespace Domain.Contracts;

public interface ICandidateExperienceRepository : IRepositoryBase<CandidateExperience>
{
    Task<IEnumerable<CandidateExperience>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default);
}

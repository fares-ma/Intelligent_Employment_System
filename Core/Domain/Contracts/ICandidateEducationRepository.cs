using Domain.Models;

namespace Domain.Contracts;

public interface ICandidateEducationRepository : IRepositoryBase<CandidateEducation>
{
    Task<IEnumerable<CandidateEducation>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default);
}

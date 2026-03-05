using Domain.Models;

namespace Domain.Contracts;

public interface ISavedJobRepository : IRepositoryBase<SavedJob>
{
    Task<SavedJob?> GetAsync(string candidateId, int jobPostId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SavedJob>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default);
}

using Domain.Models;

namespace Domain.Contracts;

public interface ICandidateAssessmentRepository : IRepositoryBase<CandidateAssessment>
{
    Task<CandidateAssessment?> GetByCandidateAndAssessmentAsync(string candidateId, int assessmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CandidateAssessment>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default);
}

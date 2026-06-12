using Domain.Models;
using Shared.Pagination;

namespace Domain.Contracts;

public interface ICandidateAssessmentRepository : IRepositoryBase<CandidateAssessment>
{
    Task<CandidateAssessment?> GetByCandidateAndAssessmentAsync(string candidateId, int assessmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CandidateAssessment>> GetByCandidateAsync(string candidateId, CancellationToken cancellationToken = default);
    Task<PagedResult<CandidateAssessment>> GetPagedByAssessmentAsync(int assessmentId, PaginationParams paginationParams, CancellationToken cancellationToken = default);
}

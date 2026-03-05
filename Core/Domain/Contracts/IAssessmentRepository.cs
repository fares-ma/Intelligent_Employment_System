using Domain.Models;

namespace Domain.Contracts;

public interface IAssessmentRepository : IRepositoryBase<Assessment>
{
    Task<Assessment?> GetWithQuestionsAsync(int assessmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assessment>> GetByJobPostAsync(int jobPostId, CancellationToken cancellationToken = default);
}

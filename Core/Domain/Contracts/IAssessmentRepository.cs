using Domain.Models;

namespace Domain.Contracts;

public interface IAssessmentRepository : IRepositoryBase<Assessment>
{
    Task<Assessment?> GetWithQuestionsAsync(int assessmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assessment>> GetByJobPostAsync(int jobPostId, CancellationToken cancellationToken = default);
    
    // Candidate attempt tracking
    Task AddAttemptAsync(CandidateAssessment attempt, CancellationToken cancellationToken = default);
    Task<CandidateAssessment?> GetAttemptAsync(int attemptId, CancellationToken cancellationToken = default);
    Task<CandidateAssessment?> GetAttemptByCandidateAndAssessmentAsync(string candidateId, int assessmentId, CancellationToken cancellationToken = default);
    void UpdateAttempt(CandidateAssessment attempt);
    Task<IEnumerable<CandidateAssessment>> GetAttemptsByAssessmentAsync(int assessmentId, CancellationToken cancellationToken = default);
}

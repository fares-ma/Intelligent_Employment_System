using Services.Abstractions.DTOs.Assessment;
using Shared.Pagination;

namespace Services.Abstractions;

/// <summary>
/// Service interface for managing candidate assessments
/// </summary>
public interface IAssessmentService
{
    // Recruiter Methods
    Task<AssessmentDetailDto> CreateAssessmentAsync(string recruiterId, CreateAssessmentDto request);
    Task<AssessmentDetailDto> UpdateAssessmentAsync(int assessmentId, string recruiterId, UpdateAssessmentDto request);
    Task DeleteAssessmentAsync(int assessmentId, string recruiterId);
    Task<IEnumerable<AssessmentListDto>> GetJobAssessmentsAsync(int jobPostId, string recruiterId);
    Task<AssessmentDetailDto> GetAssessmentAsync(int assessmentId, string userId);
    Task<PagedResult<CandidateAssessmentResultDto>> GetAssessmentResultsAsync(int assessmentId, string recruiterId, int pageNumber = 1, int pageSize = 20);

    // Candidate Methods
    Task<CandidateAssessmentAttemptDto> StartAssessmentAsync(int assessmentId, string candidateId);
    Task<SubmitAssessmentResultDto> SubmitAssessmentAsync(int assessmentId, string candidateId, SubmitAssessmentDto request);
}

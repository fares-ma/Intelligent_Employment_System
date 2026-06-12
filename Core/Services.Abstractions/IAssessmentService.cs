using Services.Abstractions.DTOs.Assessment;
using Shared.Pagination;

namespace Services.Abstractions;

public interface IAssessmentService
{
    // Recruiter / Admin Operations
    Task<AssessmentDto> CreateAssessmentAsync(CreateAssessmentDto request, string userId);
    Task<AssessmentDto> GetAssessmentAsync(int assessmentId);
    Task<IEnumerable<AssessmentDto>> GetAssessmentsForJobAsync(int jobPostId);
    Task<PagedResult<AssessmentCandidateListDto>> GetAssessmentCandidatesAsync(int assessmentId, PaginationParams pagination);
    Task<AssessmentCandidateDetailDto> GetCandidateAssessmentDetailsAsync(int assessmentId, string candidateId);
    
    // Candidate Operations
    Task<CandidateAssessmentDto> StartAssessmentAsync(int assessmentId, int jobApplicationId, string candidateId);
    Task<CandidateAssessmentDto> SubmitAssessmentAsync(SubmitAssessmentDto request, string candidateId);
    Task<CandidateAssessmentDto> GetCandidateAssessmentAsync(int assessmentId, string candidateId);
    Task<IEnumerable<CandidateAssessmentDto>> GetCandidateAssessmentsAsync(string candidateId);
}

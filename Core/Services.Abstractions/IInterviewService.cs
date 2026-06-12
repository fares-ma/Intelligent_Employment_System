using Services.Abstractions.DTOs.Interview;

namespace Services.Abstractions;

/// <summary>
/// Service for managing interviews (scheduling, conducting, completing)
/// </summary>
public interface IInterviewService
{
    /// <summary>
    /// Schedule a new interview for a job application
    /// </summary>
    Task<InterviewDto> ScheduleInterviewAsync(string recruiterId, CreateInterviewDto request);

    /// <summary>
    /// Get interview details by ID
    /// </summary>
    Task<InterviewDto> GetInterviewAsync(int interviewId);

    /// <summary>
    /// Get all interviews for a job application
    /// </summary>
    Task<IEnumerable<InterviewDto>> GetApplicationInterviewsAsync(int jobApplicationId);

    /// <summary>
    /// Get all interviews for a candidate
    /// </summary>
    Task<IEnumerable<InterviewDto>> GetCandidateInterviewsAsync(string candidateId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get all interviews for a recruiter's job postings (paginated)
    /// </summary>
    Task<Shared.Pagination.PagedResult<RecruiterInterviewDto>> GetRecruiterInterviewsAsync(string recruiterId, Domain.Enums.InterviewStatus? status, Shared.Pagination.PaginationParams pagination);

    /// <summary>
    /// Get detailed interview info for recruiter
    /// </summary>
    Task<RecruiterInterviewDto> GetInterviewDetailsAsync(int interviewId, string recruiterId);

    /// <summary>
    /// Submit evaluation for a completed interview
    /// </summary>
    Task EvaluateInterviewAsync(int interviewId, string recruiterId, EvaluateInterviewDto evaluation);

    /// <summary>
    /// Update interview (reschedule or change details)
    /// </summary>
    Task<InterviewDto> UpdateInterviewAsync(int interviewId, string recruiterId, UpdateInterviewDto request);

    /// <summary>
    /// Cancel an interview (must be in Scheduled status)
    /// </summary>
    Task CancelInterviewAsync(int interviewId, string recruiterId);

    /// <summary>
    /// Get interviews by status (Scheduled, InProgress, Completed, Cancelled)
    /// </summary>
    Task<IEnumerable<InterviewDto>> GetInterviewsByStatusAsync(string status, int pageNumber = 1, int pageSize = 20);
}

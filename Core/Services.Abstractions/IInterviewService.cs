using Services.Abstractions.DTOs.Interview;
using Shared.Pagination;

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
    Task<IEnumerable<InterviewDto>> GetRecruiterInterviewsAsync(string recruiterId, int pageNumber = 1, int pageSize = 20);

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

    /// <summary>
    /// Get AI-generated questions for an interview
    /// </summary>
    Task<AiInterviewQuestionsDto> GetAiQuestionsAsync(int interviewId, string userId);

    /// <summary>
    /// Submit written answers for AI interview
    /// </summary>
    Task<InterviewDto> SubmitAiAnswersAsync(int interviewId, string userId, SubmitAiInterviewDto request);

    /// <summary>
    /// Create a live interview room (via third-party WebRTC provider)
    /// </summary>
    Task<InterviewDto> CreateLiveRoomAsync(int interviewId, string recruiterId);

    /// <summary>
    /// Complete an interview (set score and feedback)
    /// </summary>
    Task<InterviewDto> CompleteInterviewAsync(int interviewId, string recruiterId, CompleteInterviewDto request);
}

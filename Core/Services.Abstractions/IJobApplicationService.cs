using Services.Abstractions.DTOs.JobApplication;

namespace Services.Abstractions;

/// <summary>
/// Service interface for managing job applications
/// </summary>
public interface IJobApplicationService
{
    /// <summary>
    /// Submit a new job application
    /// </summary>
    Task<JobApplicationDto> ApplyForJobAsync(string candidateId, CreateJobApplicationDto request);

    /// <summary>
    /// Get a specific job application by ID
    /// </summary>
    Task<JobApplicationDto> GetApplicationAsync(int applicationId);

    /// <summary>
    /// Get all applications submitted by a candidate
    /// </summary>
    Task<IEnumerable<JobApplicationDto>> GetCandidateApplicationsAsync(string candidateId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get all applications received for a job posting
    /// </summary>
    Task<IEnumerable<JobApplicationDto>> GetJobApplicationsAsync(int jobPostId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Update application status (recruiter/admin only)
    /// </summary>
    Task<JobApplicationDto> UpdateApplicationStatusAsync(int applicationId, string recruiterId, bool isAdmin, UpdateJobApplicationDto request);

    /// <summary>
    /// Withdraw application (candidate only)
    /// </summary>
    Task WithdrawApplicationAsync(int applicationId, string candidateId);

    /// <summary>
    /// Check if candidate already applied for a job
    /// </summary>
    Task<bool> HasAppliedAsync(string candidateId, int jobPostId);

    /// <summary>
    /// Get applications with specific status for a job posting
    /// </summary>
    Task<IEnumerable<JobApplicationDto>> GetApplicationsByStatusAsync(int jobPostId, string status, int pageNumber = 1, int pageSize = 20);
}

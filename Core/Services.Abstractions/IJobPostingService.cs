using Services.Abstractions.DTOs.JobPosting;
using Shared.Pagination;

namespace Services.Abstractions;

/// <summary>
/// Service interface for managing job postings
/// </summary>
public interface IJobPostingService
{
    /// <summary>
    /// Get all active job postings with pagination
    /// </summary>
    Task<IEnumerable<JobPostingDto>> GetAllActiveJobPostingsAsync(int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get job postings by company ID
    /// </summary>
    Task<IEnumerable<JobPostingDto>> GetCompanyJobPostingsAsync(int companyId, int pageNumber = 1, int pageSize = 20);
    
    /// <summary>
    /// Retrieves applicants for a specific job post with filtering and sorting
    /// </summary>
    Task<PagedResult<Services.Abstractions.DTOs.JobPosting.JobApplicantDto>> GetJobApplicantsAsync(int jobPostId, string recruiterId, PaginationParams pagination, Domain.Enums.ApplicationStatus? status, string? sortBy); 

    /// <summary>
    /// Get a specific job posting by ID
    /// </summary>
    Task<JobPostingDto> GetJobPostingAsync(int jobPostingId);

    /// <summary>
    /// Create a new job posting (admin/recruiter only)
    /// </summary>
    Task<JobPostingDto> CreateJobPostingAsync(int companyId, string recruiterId, CreateJobPostingDto request);

    /// <summary>
    /// Update an existing job posting (admin/recruiter only)
    /// </summary>
    Task<JobPostingDto> UpdateJobPostingAsync(int jobPostingId, string recruiterId, UpdateJobPostingDto request);

    /// <summary>
    /// Delete/deactivate a job posting (admin/recruiter only)
    /// </summary>
    Task DeleteJobPostingAsync(int jobPostingId, string recruiterId);

    /// <summary>
    /// Search job postings by title, description, or location
    /// </summary>
    Task<IEnumerable<JobPostingDto>> SearchJobPostingsAsync(string searchTerm, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get job postings that require specific skills
    /// </summary>
    Task<IEnumerable<JobPostingDto>> GetJobPostingsBySkillAsync(int skillId, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Get job postings by employment type
    /// </summary>
    Task<IEnumerable<JobPostingDto>> GetJobPostingsByTypeAsync(string employmentType, int pageNumber = 1, int pageSize = 20);
}

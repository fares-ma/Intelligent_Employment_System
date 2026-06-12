using Services.Abstractions.DTOs.Candidates;
using Shared;
using Shared.Pagination;

namespace Services.Abstractions;

public interface ICandidateService
{
    /// <summary>
    /// Get candidate full profile with skills and resumes
    /// </summary>
    Task<CandidateProfileDto> GetProfileAsync(string candidateId);

    /// <summary>
    /// Update candidate profile information
    /// </summary>
    Task<CandidateProfileDto> UpdateProfileAsync(string candidateId, UpdateCandidateProfileDto dto);
    
    /// <summary>
    /// Gets the candidate profile for a recruiter (verifies the candidate applied to the recruiter's jobs)
    /// </summary>
    Task<Services.Abstractions.DTOs.Candidate.RecruiterCandidateProfileDto> GetProfileForRecruiterAsync(string candidateId, string recruiterId);

    /// <summary>
    /// Update candidate skills (replaces existing)
    /// </summary>
    Task UpdateSkillsAsync(string candidateId, UpdateSkillsDto dto);

    /// <summary>
    /// Upload resume file with validation
    /// </summary>
    Task<ResumeDto> UploadResumeAsync(string candidateId, string fileName, Stream fileStream);

    /// <summary>
    /// Download resume
    /// </summary>
    Task<(Stream stream, string contentType, string fileName)> DownloadResumeAsync(string candidateId, int resumeId);

    /// <summary>
    /// Delete resume
    /// </summary>
    Task DeleteResumeAsync(string candidateId, int resumeId);

    /// <summary>
    /// Get candidate's job applications
    /// </summary>
    Task<PagedResult<CandidateApplicationDto>> GetApplicationsAsync(string candidateId, PaginationParams paginationParams);

    /// <summary>
    /// Get candidate's saved jobs
    /// </summary>
    Task<PagedResult<dynamic>> GetSavedJobsAsync(string candidateId, PaginationParams paginationParams);

    /// <summary>
    /// Toggle save/unsave a job
    /// </summary>
    Task ToggleSaveJobAsync(string candidateId, int jobPostId);

    /// <summary>
    /// Update profile picture
    /// </summary>
    Task<string> UpdateProfilePictureAsync(string candidateId, string fileName, Stream fileStream);
}

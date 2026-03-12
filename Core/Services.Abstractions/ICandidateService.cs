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
    /// Update candidate skills (replaces existing)
    /// </summary>
    Task UpdateSkillsAsync(string candidateId, UpdateSkillsDto dto);

    /// <summary>
    /// Upload resume file with validation and optional AI processing
    /// </summary>
    Task<ResumeDto> UploadResumeAsync(string candidateId, string fileName, Stream fileStream);

    /// <summary>
    /// Delete resume
    /// </summary>
    Task DeleteResumeAsync(string candidateId, int resumeId);

    /// <summary>
    /// Generate AI CV from resume
    /// </summary>
    Task<string> GenerateCvAsync(string candidateId, int resumeId);

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

using Services.Abstractions.DTOs.Candidate;

namespace Services.Abstractions;

/// <summary>
/// Service interface for managing candidate work experience records
/// </summary>
public interface IExperienceService
{
    /// <summary>
    /// Add a new experience record to candidate profile
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="request">Experience details</param>
    /// <returns>Created experience record</returns>
    /// <exception cref="NotFoundException">Thrown when candidate not found</exception>
    /// <exception cref="BadRequestException">Thrown when validation fails</exception>
    Task<CandidateExperienceDto> AddExperienceAsync(string candidateId, CreateExperienceDto request);

    /// <summary>
    /// Get all experience records for a candidate
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <returns>List of experience records ordered by start date descending</returns>
    /// <exception cref="NotFoundException">Thrown when candidate not found</exception>
    Task<IEnumerable<CandidateExperienceDto>> GetCandidateExperiencesAsync(string candidateId);

    /// <summary>
    /// Get a specific experience record
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="experienceId">Experience record ID</param>
    /// <returns>Experience record details</returns>
    /// <exception cref="NotFoundException">Thrown when experience or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the experience record</exception>
    Task<CandidateExperienceDto> GetExperienceAsync(string candidateId, string experienceId);

    /// <summary>
    /// Update an experience record
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="experienceId">Experience record ID</param>
    /// <param name="request">Updated experience details</param>
    /// <returns>Updated experience record</returns>
    /// <exception cref="NotFoundException">Thrown when experience or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the experience record</exception>
    /// <exception cref="BadRequestException">Thrown when validation fails</exception>
    Task<CandidateExperienceDto> UpdateExperienceAsync(string candidateId, string experienceId, UpdateExperienceDto request);

    /// <summary>
    /// Delete an experience record
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="experienceId">Experience record ID</param>
    /// <exception cref="NotFoundException">Thrown when experience or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the experience record</exception>
    Task DeleteExperienceAsync(string candidateId, string experienceId);
}

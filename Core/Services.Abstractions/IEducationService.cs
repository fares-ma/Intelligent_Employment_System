using Services.Abstractions.DTOs.Candidate;

namespace Services.Abstractions;

/// <summary>
/// Service interface for managing candidate education records
/// </summary>
public interface IEducationService
{
    /// <summary>
    /// Add a new education record to candidate profile
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="request">Education details</param>
    /// <returns>Created education record</returns>
    /// <exception cref="NotFoundException">Thrown when candidate not found</exception>
    /// <exception cref="BadRequestException">Thrown when validation fails</exception>
    Task<CandidateEducationDto> AddEducationAsync(string candidateId, CreateEducationDto request);

    /// <summary>
    /// Get all education records for a candidate
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <returns>List of education records</returns>
    /// <exception cref="NotFoundException">Thrown when candidate not found</exception>
    Task<IEnumerable<CandidateEducationDto>> GetCandidateEducationsAsync(string candidateId);

    /// <summary>
    /// Get a specific education record
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="educationId">Education record ID</param>
    /// <returns>Education record details</returns>
    /// <exception cref="NotFoundException">Thrown when education or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the education record</exception>
    Task<CandidateEducationDto> GetEducationAsync(string candidateId, string educationId);

    /// <summary>
    /// Update an education record
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="educationId">Education record ID</param>
    /// <param name="request">Updated education details</param>
    /// <returns>Updated education record</returns>
    /// <exception cref="NotFoundException">Thrown when education or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the education record</exception>
    /// <exception cref="BadRequestException">Thrown when validation fails</exception>
    Task<CandidateEducationDto> UpdateEducationAsync(string candidateId, string educationId, UpdateEducationDto request);

    /// <summary>
    /// Delete an education record
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="educationId">Education record ID</param>
    /// <exception cref="NotFoundException">Thrown when education or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the education record</exception>
    Task DeleteEducationAsync(string candidateId, string educationId);
}

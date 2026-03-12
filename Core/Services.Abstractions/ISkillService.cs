using Services.Abstractions.DTOs.Candidate;

namespace Services.Abstractions;

/// <summary>
/// Service interface for managing candidate skills and proficiency levels
/// </summary>
public interface ISkillService
{
    /// <summary>
    /// Add a new skill to candidate profile
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="request">Skill details including proficiency level</param>
    /// <returns>Created skill record</returns>
    /// <exception cref="NotFoundException">Thrown when candidate not found</exception>
    /// <exception cref="BadRequestException">Thrown when validation fails or skill already exists</exception>
    Task<CandidateSkillDto> AddSkillAsync(string candidateId, CreateSkillDto request);

    /// <summary>
    /// Get all skills for a candidate
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <returns>List of candidate skills with levels</returns>
    /// <exception cref="NotFoundException">Thrown when candidate not found</exception>
    Task<IEnumerable<CandidateSkillDto>> GetCandidateSkillsAsync(string candidateId);

    /// <summary>
    /// Get a specific skill record
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="skillId">Skill record ID</param>
    /// <returns>Skill record details</returns>
    /// <exception cref="NotFoundException">Thrown when skill or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the skill</exception>
    Task<CandidateSkillDto> GetSkillAsync(string candidateId, string skillId);

    /// <summary>
    /// Update skill proficiency level
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="skillId">Skill record ID</param>
    /// <param name="request">Updated proficiency level</param>
    /// <returns>Updated skill record</returns>
    /// <exception cref="NotFoundException">Thrown when skill or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the skill</exception>
    Task<CandidateSkillDto> UpdateSkillLevelAsync(string candidateId, string skillId, UpdateSkillDto request);

    /// <summary>
    /// Remove a skill from candidate profile
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="skillId">Skill record ID</param>
    /// <exception cref="NotFoundException">Thrown when skill or candidate not found</exception>
    /// <exception cref="ForbiddenException">Thrown when candidate doesn't own the skill</exception>
    Task DeleteSkillAsync(string candidateId, string skillId);

    /// <summary>
    /// Get skills by proficiency level for a candidate
    /// Levels: 1=Beginner, 2=Intermediate, 3=Expert
    /// </summary>
    /// <param name="candidateId">Candidate user ID</param>
    /// <param name="level">Proficiency level to filter by (1-3)</param>
    /// <returns>List of skills at specified level</returns>
    Task<IEnumerable<CandidateSkillDto>> GetSkillsByLevelAsync(string candidateId, int level);
}

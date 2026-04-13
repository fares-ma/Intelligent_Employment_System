using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Candidate;
using System.Security.Claims;

namespace IES.api.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExperienceController : ControllerBase
{
    private readonly IExperienceService _experienceService;
    private readonly ILogger<ExperienceController> _logger;

    public ExperienceController(IExperienceService experienceService, ILogger<ExperienceController> logger)
    {
        _experienceService = experienceService ?? throw new ArgumentNullException(nameof(experienceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all work experience records for the current candidate
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CandidateExperienceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExperiences()
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var experiences = await _experienceService.GetCandidateExperiencesAsync(candidateId);
            return Ok(experiences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving experiences");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific work experience record
    /// </summary>
    [HttpGet("{experienceId}")]
    [ProducesResponseType(typeof(CandidateExperienceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExperience(string experienceId)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var experience = await _experienceService.GetExperienceAsync(candidateId, experienceId);
            return Ok(experience);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving experience {ExperienceId}", experienceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Add a new work experience record
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CandidateExperienceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddExperience([FromBody] CreateExperienceDto request)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var experience = await _experienceService.AddExperienceAsync(candidateId, request);
            return CreatedAtAction(nameof(GetExperience), new { experienceId = experience.Id }, experience);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request adding experience");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding experience");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a work experience record
    /// </summary>
    [HttpPut("{experienceId}")]
    [ProducesResponseType(typeof(CandidateExperienceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateExperience(string experienceId, [FromBody] UpdateExperienceDto request)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var experience = await _experienceService.UpdateExperienceAsync(candidateId, experienceId, request);
            return Ok(experience);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request updating experience");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating experience {ExperienceId}", experienceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a work experience record
    /// </summary>
    [HttpDelete("{experienceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteExperience(string experienceId)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            await _experienceService.DeleteExperienceAsync(candidateId, experienceId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting experience {ExperienceId}", experienceId);
            return StatusCode(500, "Internal server error");
        }
    }
}

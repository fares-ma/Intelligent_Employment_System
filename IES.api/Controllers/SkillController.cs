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
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly ILogger<SkillController> _logger;

    public SkillController(ISkillService skillService, ILogger<SkillController> logger)
    {
        _skillService = skillService ?? throw new ArgumentNullException(nameof(skillService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all skills for the current candidate
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CandidateSkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSkills()
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var skills = await _skillService.GetCandidateSkillsAsync(candidateId);
            return Ok(skills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skills");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific skill
    /// </summary>
    [HttpGet("{skillId}")]
    [ProducesResponseType(typeof(CandidateSkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSkill(string skillId)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var skill = await _skillService.GetSkillAsync(candidateId, skillId);
            return Ok(skill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill {SkillId}", skillId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Add a new skill
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CandidateSkillDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddSkill([FromBody] CreateSkillDto request)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var skill = await _skillService.AddSkillAsync(candidateId, request);
            return CreatedAtAction(nameof(GetSkill), new { skillId = skill.Id }, skill);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request adding skill");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding skill");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a skill's proficiency level
    /// </summary>
    [HttpPut("{skillId}")]
    [ProducesResponseType(typeof(CandidateSkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSkill(string skillId, [FromBody] UpdateSkillDto request)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var skill = await _skillService.UpdateSkillLevelAsync(candidateId, skillId, request);
            return Ok(skill);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request updating skill");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill {SkillId}", skillId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a skill
    /// </summary>
    [HttpDelete("{skillId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSkill(string skillId)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            await _skillService.DeleteSkillAsync(candidateId, skillId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting skill {SkillId}", skillId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get skills by proficiency level (1=Beginner, 2=Intermediate, 3=Expert)
    /// </summary>
    [HttpGet("level/{level}")]
    [ProducesResponseType(typeof(IEnumerable<CandidateSkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSkillsByLevel(int level)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var skills = await _skillService.GetSkillsByLevelAsync(candidateId, level);
            return Ok(skills);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request getting skills by level");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skills by level {Level}", level);
            return StatusCode(500, "Internal server error");
        }
    }
}

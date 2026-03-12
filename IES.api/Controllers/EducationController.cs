using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Candidate;
using System.Security.Claims;

namespace IES.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EducationController : ControllerBase
{
    private readonly IEducationService _educationService;
    private readonly ILogger<EducationController> _logger;

    public EducationController(IEducationService educationService, ILogger<EducationController> logger)
    {
        _educationService = educationService ?? throw new ArgumentNullException(nameof(educationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all educational qualifications for the current candidate
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CandidateEducationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEducations()
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var educations = await _educationService.GetCandidateEducationsAsync(candidateId);
            return Ok(educations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving educations");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific educational qualification
    /// </summary>
    [HttpGet("{educationId}")]
    [ProducesResponseType(typeof(CandidateEducationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEducation(string educationId)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var education = await _educationService.GetEducationAsync(candidateId, educationId);
            return Ok(education);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving education {EducationId}", educationId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Add a new educational qualification
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CandidateEducationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddEducation([FromBody] CreateEducationDto request)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var education = await _educationService.AddEducationAsync(candidateId, request);
            return CreatedAtAction(nameof(GetEducation), new { educationId = education.Id }, education);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request adding education");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding education");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an educational qualification
    /// </summary>
    [HttpPut("{educationId}")]
    [ProducesResponseType(typeof(CandidateEducationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEducation(string educationId, [FromBody] UpdateEducationDto request)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            var education = await _educationService.UpdateEducationAsync(candidateId, educationId, request);
            return Ok(education);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request updating education");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating education {EducationId}", educationId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete an educational qualification
    /// </summary>
    [HttpDelete("{educationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEducation(string educationId)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(candidateId))
                return Unauthorized("User ID not found in token");

            await _educationService.DeleteEducationAsync(candidateId, educationId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting education {EducationId}", educationId);
            return StatusCode(500, "Internal server error");
        }
    }
}

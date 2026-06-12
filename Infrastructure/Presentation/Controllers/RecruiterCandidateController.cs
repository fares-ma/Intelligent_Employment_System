using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Candidate;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/Candidates/recruiter")]
[Authorize(Roles = "Recruiter,Admin")]
[Produces("application/json")]
public class RecruiterCandidateController : ControllerBase
{
    private readonly ICandidateService _candidateService;

    public RecruiterCandidateController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    /// <summary>
    /// Get full candidate profile for recruiters
    /// </summary>
    [HttpGet("profile/{candidateId}")]
    [ProducesResponseType(typeof(RecruiterCandidateProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecruiterCandidateProfileDto>> GetProfileForRecruiter(string candidateId)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(recruiterId))
                return Unauthorized("User ID not found in token");

            var profile = await _candidateService.GetProfileForRecruiterAsync(candidateId, recruiterId);
            return Ok(profile);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Domain.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

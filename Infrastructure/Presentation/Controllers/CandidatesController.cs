using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Candidates;
using Shared;
using Shared.Pagination;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Candidate")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;
    private readonly ILogger<CandidatesController> _logger;

    public CandidatesController(ICandidateService candidateService, ILogger<CandidatesController> logger)
    {
        _candidateService = candidateService;
        _logger = logger;
    }

    /// <summary>
    /// Get candidate profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<CandidateProfileDto>> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profile = await _candidateService.GetProfileAsync(userId);
        return Ok(profile);
    }

    /// <summary>
    /// Update candidate profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<ActionResult<CandidateProfileDto>> UpdateProfile([FromBody] UpdateCandidateProfileDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profile = await _candidateService.UpdateProfileAsync(userId, dto);
        return Ok(profile);
    }

    /// <summary>
    /// Update candidate skills
    /// </summary>
    [HttpPut("skills")]
    public async Task<IActionResult> UpdateSkills([FromBody] UpdateSkillsDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _candidateService.UpdateSkillsAsync(userId, dto);
        return Ok();
    }

    /// <summary>
    /// Upload resume (PDF or DOCX)
    /// </summary>
    [HttpPost("resume")]
    public async Task<ActionResult<ResumeDto>> UploadResume(IFormFile file)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        using (var stream = file.OpenReadStream())
        {
            var resume = await _candidateService.UploadResumeAsync(userId, file.FileName, stream);
            return CreatedAtAction(nameof(GetProfile), resume);
        }
    }

    /// <summary>
    /// Delete resume
    /// </summary>
    [HttpDelete("resume/{resumeId:int}")]
    public async Task<IActionResult> DeleteResume(int resumeId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _candidateService.DeleteResumeAsync(userId, resumeId);
        return NoContent();
    }

    /// <summary>
    /// Generate AI CV from resume
    /// </summary>
    [HttpPost("resume/{resumeId:int}/generate-cv")]
    public async Task<ActionResult<string>> GenerateCv(int resumeId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var cvPath = await _candidateService.GenerateCvAsync(userId, resumeId);
        return Ok(new { cvPath });
    }

    /// <summary>
    /// Update profile picture
    /// </summary>
    [HttpPut("profile-picture")]
    public async Task<ActionResult<string>> UpdateProfilePicture(IFormFile file)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        using (var stream = file.OpenReadStream())
        {
            var picturePath = await _candidateService.UpdateProfilePictureAsync(userId, file.FileName, stream);
            return Ok(new { picturePath });
        }
    }

    /// <summary>
    /// Get candidate applications
    /// </summary>
    [HttpGet("applications")]
    public async Task<ActionResult<PagedResult<CandidateApplicationDto>>> GetApplications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var applications = await _candidateService.GetApplicationsAsync(userId, paginationParams);
        return Ok(applications);
    }

    /// <summary>
    /// Get saved jobs
    /// </summary>
    [HttpGet("saved-jobs")]
    public async Task<ActionResult<PagedResult<dynamic>>> GetSavedJobs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var savedJobs = await _candidateService.GetSavedJobsAsync(userId, paginationParams);
        return Ok(savedJobs);
    }

    /// <summary>
    /// Toggle save job
    /// </summary>
    [HttpPost("saved-jobs/{jobPostId:int}")]
    public async Task<IActionResult> ToggleSaveJob(int jobPostId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _candidateService.ToggleSaveJobAsync(userId, jobPostId);
        return Ok();
    }
}

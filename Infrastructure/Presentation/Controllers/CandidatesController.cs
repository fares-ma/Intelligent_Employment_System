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

    // Validation constants
    private static readonly string[] AllowedResumeExtensions = { ".pdf", ".docx" };
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };
    private const long MaxResumeSize = 10 * 1024 * 1024; // 10MB
    private const long MaxImageSize = 5 * 1024 * 1024;   // 5MB
    private const int MaxPageSize = 100;

    public CandidatesController(ICandidateService candidateService, ILogger<CandidatesController> logger)
    {
        _candidateService = candidateService;
        _logger = logger;
    }

    /// <summary>
    /// Get candidate profile
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CandidateProfileDto>> UpdateProfile([FromBody] UpdateCandidateProfileDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _logger.LogInformation("Updating profile for candidate {UserId}", userId);
        var profile = await _candidateService.UpdateProfileAsync(userId, dto);
        return Ok(profile);
    }

    /// <summary>
    /// Update candidate skills
    /// </summary>
    [HttpPut("skills")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(typeof(ResumeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResumeDto>> UploadResume(IFormFile file)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Validate file
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required and cannot be empty" });

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedResumeExtensions.Contains(extension))
            return BadRequest(new { message = $"Only {string.Join(", ", AllowedResumeExtensions)} files are allowed" });

        if (file.Length > MaxResumeSize)
            return BadRequest(new { message = $"File size cannot exceed {MaxResumeSize / (1024 * 1024)}MB" });

        _logger.LogInformation("Resume upload started for candidate {UserId}: {FileName} ({Size} bytes)", userId, file.FileName, file.Length);

        using (var stream = file.OpenReadStream())
        {
            var resume = await _candidateService.UploadResumeAsync(userId, file.FileName, stream);
            _logger.LogInformation("Resume upload completed for candidate {UserId}: ResumeId={ResumeId}", userId, resume.Id);
            return CreatedAtAction(nameof(GetProfile), resume);
        }
    }

    /// <summary>
    /// Delete resume
    /// </summary>
    [HttpDelete("resume/{resumeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteResume(int resumeId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        _logger.LogInformation("Deleting resume {ResumeId} for candidate {UserId}", resumeId, userId);
        await _candidateService.DeleteResumeAsync(userId, resumeId);
        return NoContent();
    }

    /// <summary>
    /// Update profile picture
    /// </summary>
    [HttpPut("profile-picture")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> UpdateProfilePicture(IFormFile file)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Validate file
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Image file is required and cannot be empty" });

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedImageExtensions.Contains(extension))
            return BadRequest(new { message = $"Only {string.Join(", ", AllowedImageExtensions)} images are allowed" });

        if (file.Length > MaxImageSize)
            return BadRequest(new { message = $"Image size cannot exceed {MaxImageSize / (1024 * 1024)}MB" });

        _logger.LogInformation("Profile picture upload for candidate {UserId}: {FileName}", userId, file.FileName);

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
    [ProducesResponseType(typeof(PagedResult<CandidateApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<CandidateApplicationDto>>> GetApplications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (pageNumber < 1)
            return BadRequest(new { message = "pageNumber must be >= 1" });
        if (pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest(new { message = $"pageSize must be between 1 and {MaxPageSize}" });

        var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var applications = await _candidateService.GetApplicationsAsync(userId, paginationParams);
        return Ok(applications);
    }

    /// <summary>
    /// Get saved jobs
    /// </summary>
    [HttpGet("saved-jobs")]
    [ProducesResponseType(typeof(PagedResult<dynamic>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<dynamic>>> GetSavedJobs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (pageNumber < 1)
            return BadRequest(new { message = "pageNumber must be >= 1" });
        if (pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest(new { message = $"pageSize must be between 1 and {MaxPageSize}" });

        var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var savedJobs = await _candidateService.GetSavedJobsAsync(userId, paginationParams);
        return Ok(savedJobs);
    }

    /// <summary>
    /// Toggle save job
    /// </summary>
    [HttpPost("saved-jobs/{jobPostId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleSaveJob(int jobPostId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _candidateService.ToggleSaveJobAsync(userId, jobPostId);
        return Ok();
    }
}

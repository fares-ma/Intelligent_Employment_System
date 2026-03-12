using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Interview;
using System.Security.Claims;

namespace IES.api.Controllers;

/// <summary>
/// API endpoints for managing interviews
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterviewController : ControllerBase
{
    private readonly IInterviewService _service;
    private readonly ILogger<InterviewController> _logger;

    public InterviewController(IInterviewService service, ILogger<InterviewController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Schedule a new interview (Recruiter only)
    /// </summary>
    [HttpPost("schedule")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<InterviewDto>> ScheduleInterview([FromBody] CreateInterviewDto request)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User identity not found");

            var result = await _service.ScheduleInterviewAsync(recruiterId, request);
            return CreatedAtAction(nameof(GetInterview), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in ScheduleInterview: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authorization error in ScheduleInterview: {Message}", ex.Message);
            return Forbid("You don't have permission to schedule interviews for this job posting");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ScheduleInterview");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get interview details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InterviewDto>> GetInterview(int id)
    {
        try
        {
            var result = await _service.GetInterviewAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Interview not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetInterview");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all interviews for a job application
    /// </summary>
    [HttpGet("application/{applicationId}")]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetApplicationInterviews(int applicationId)
    {
        try
        {
            var result = await _service.GetApplicationInterviewsAsync(applicationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetApplicationInterviews");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all interviews for a candidate
    /// </summary>
    [HttpGet("candidate/{candidateId}")]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetCandidateInterviews(
        string candidateId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Candidates can only view their own interviews
            if (userRole == "Candidate" && currentUserId != candidateId)
                return Forbid("You can only view your own interviews");

            var result = await _service.GetCandidateInterviewsAsync(candidateId, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in GetCandidateInterviews: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCandidateInterviews");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all interviews for recruiter's job postings
    /// </summary>
    [HttpGet("recruiter/interviews")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetRecruiterInterviews(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User identity not found");

            var result = await _service.GetRecruiterInterviewsAsync(recruiterId, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in GetRecruiterInterviews: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetRecruiterInterviews");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get interviews by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<IEnumerable<InterviewDto>>> GetInterviewsByStatus(
        string status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _service.GetInterviewsByStatusAsync(status, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in GetInterviewsByStatus: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetInterviewsByStatus");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update interview (reschedule or change status)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<InterviewDto>> UpdateInterview(
        int id,
        [FromBody] UpdateInterviewDto request)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User identity not found");

            var result = await _service.UpdateInterviewAsync(id, recruiterId, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authorization error in UpdateInterview: {Message}", ex.Message);
            return Forbid("You don't have permission to update this interview");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in UpdateInterview: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateInterview");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel an interview (Recruiter only)
    /// </summary>
    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> CancelInterview(int id)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User identity not found");

            await _service.CancelInterviewAsync(id, recruiterId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authorization error in CancelInterview: {Message}", ex.Message);
            return Forbid("You don't have permission to cancel this interview");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in CancelInterview: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CancelInterview");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

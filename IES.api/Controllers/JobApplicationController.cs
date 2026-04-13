using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using System.Globalization;
using System.IO;
using Services.Abstractions;
using Services.Abstractions.DTOs.JobApplication;
using System.Security.Claims;

namespace IES.api.Controllers;

/// <summary>
/// API endpoints for managing job applications
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class JobApplicationController : ControllerBase
{
    private readonly IJobApplicationService _service;
    private readonly ILogger<JobApplicationController> _logger;

    public JobApplicationController(IJobApplicationService service, ILogger<JobApplicationController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Submit a new job application (Candidate only)
    /// </summary>
    [HttpPost("apply")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<JobApplicationDto>> ApplyForJob([FromBody] CreateJobApplicationDto request)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User identity not found");

            var result = await _service.ApplyForJobAsync(candidateId, request);
            return CreatedAtAction(nameof(GetApplication), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in ApplyForJob: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ApplyForJob");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a specific job application by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplicationDto>> GetApplication(int id)
    {
        try
        {
            var result = await _service.GetApplicationAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Application not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetApplication");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all applications from a candidate (Candidate can view their own, Recruiter/Admin can view all)
    /// </summary>
    [HttpGet("candidate/{candidateId}")]
    public async Task<ActionResult<IEnumerable<JobApplicationDto>>> GetCandidateApplications(
        string candidateId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Candidates can only view their own applications
            if (User.IsInRole("Candidate") && currentUserId != candidateId)
                return Forbid("You can only view your own applications");

            var result = await _service.GetCandidateApplicationsAsync(candidateId, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in GetCandidateApplications: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCandidateApplications");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all applications for a job posting (Recruiter/Admin only)
    /// </summary>
    [HttpGet("job/{jobPostId}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<IEnumerable<JobApplicationDto>>> GetJobApplications(
        int jobPostId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _service.GetJobApplicationsAsync(jobPostId, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in GetJobApplications: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetJobApplications");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Export job applications to CSV (Recruiter/Admin only)
    /// </summary>
    [HttpGet("job/{jobPostId}/export")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> ExportApplicationsAsCsv(int jobPostId)
    {
        try
        {
            // Fetch applications without pagination for export
            var applications = await _service.GetJobApplicationsAsync(jobPostId, 1, 10000); // Hacky way for all for MVP

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            var exportData = applications.Select(a => new
            {
                ApplicationId = a.Id,
                CandidateName = a.CandidateName,
                CandidateId = a.CandidateId,
                Status = a.Status,
                AppliedAt = a.AppliedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                Score = a.MatchScore?.ToString() ?? "N/A"
            }).ToList();

            await csvWriter.WriteRecordsAsync(exportData);
            await csvWriter.FlushAsync();
            await streamWriter.FlushAsync();
            
            memoryStream.Position = 0;

            return File(memoryStream.ToArray(), "text/csv", $"job-{jobPostId}-applications.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting applications");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get applications by status for a job posting (Recruiter/Admin only)
    /// </summary>
    [HttpGet("job/{jobPostId}/status/{status}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<IEnumerable<JobApplicationDto>>> GetApplicationsByStatus(
        int jobPostId,
        string status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _service.GetApplicationsByStatusAsync(jobPostId, status, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in GetApplicationsByStatus: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetApplicationsByStatus");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Check if a candidate has already applied for a job
    /// </summary>
    [HttpGet("check/{candidateId}/{jobPostId}")]
    public async Task<ActionResult<bool>> HasApplied(string candidateId, int jobPostId)
    {
        try
        {
            var result = await _service.HasAppliedAsync(candidateId, jobPostId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HasApplied");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update application status (Recruiter/Admin only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<JobApplicationDto>> UpdateApplicationStatus(
        int id,
        [FromBody] UpdateJobApplicationDto request)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User identity not found");

            var result = await _service.UpdateApplicationStatusAsync(id, recruiterId, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authorization error in UpdateApplicationStatus: {Message}", ex.Message);
            return Forbid("You don't have permission to update this application");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in UpdateApplicationStatus: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateApplicationStatus");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Withdraw an application (Candidate only, must be in Pending status)
    /// </summary>
    [HttpDelete("{id}/withdraw")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> WithdrawApplication(int id)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User identity not found");

            await _service.WithdrawApplicationAsync(id, candidateId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authorization error in WithdrawApplication: {Message}", ex.Message);
            return Forbid("You can only withdraw your own applications");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in WithdrawApplication: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WithdrawApplication");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

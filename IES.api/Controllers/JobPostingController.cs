using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.JobPosting;
using System.Security.Claims;

namespace IES.api.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JobPostingController : ControllerBase
{
    private readonly IJobPostingService _jobPostingService;
    private readonly ILogger<JobPostingController> _logger;

    public JobPostingController(IJobPostingService jobPostingService, ILogger<JobPostingController> logger)
    {
        _jobPostingService = jobPostingService ?? throw new ArgumentNullException(nameof(jobPostingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all active job postings with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JobPostingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllJobPostings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var jobPostings = await _jobPostingService.GetAllActiveJobPostingsAsync(pageNumber, pageSize);
            return Ok(jobPostings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request getting job postings");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job postings");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific job posting
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JobPostingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetJobPosting(int id)
    {
        try
        {
            var jobPosting = await _jobPostingService.GetJobPostingAsync(id);
            return Ok(jobPosting);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Job posting not found: {JobPostingId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job posting {JobPostingId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get job postings by company (recruiter only)
    /// </summary>
    [HttpGet("company/{companyId}")]
    [ProducesResponseType(typeof(IEnumerable<JobPostingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompanyJobPostings(int companyId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var jobPostings = await _jobPostingService.GetCompanyJobPostingsAsync(companyId, pageNumber, pageSize);
            return Ok(jobPostings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request getting company job postings");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company job postings");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new job posting (recruiter only)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(JobPostingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateJobPosting([FromQuery] int companyId, [FromBody] CreateJobPostingDto request)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(recruiterId))
                return Unauthorized("User ID not found in token");

            var jobPosting = await _jobPostingService.CreateJobPostingAsync(companyId, recruiterId, request);
            return CreatedAtAction(nameof(GetJobPosting), new { id = jobPosting.Id }, jobPosting);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request creating job posting");
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized creating job posting");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job posting");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a job posting (recruiter only)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(JobPostingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateJobPosting(int id, [FromBody] UpdateJobPostingDto request)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(recruiterId))
                return Unauthorized("User ID not found in token");

            var jobPosting = await _jobPostingService.UpdateJobPostingAsync(id, recruiterId, request);
            return Ok(jobPosting);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request updating job posting");
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized updating job posting");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job posting {JobPostingId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a job posting (recruiter only)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteJobPosting(int id)
    {
        try
        {
            var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(recruiterId))
                return Unauthorized("User ID not found in token");

            await _jobPostingService.DeleteJobPostingAsync(id, recruiterId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Job posting not found: {JobPostingId}", id);
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized deleting job posting");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job posting {JobPostingId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search job postings by title, description, or location
    /// </summary>
    [HttpGet("search/{searchTerm}")]
    [ProducesResponseType(typeof(IEnumerable<JobPostingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchJobPostings(string searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var jobPostings = await _jobPostingService.SearchJobPostingsAsync(searchTerm, pageNumber, pageSize);
            return Ok(jobPostings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request searching job postings");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching job postings");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get job postings by required skill
    /// </summary>
    [HttpGet("skill/{skillId}")]
    [ProducesResponseType(typeof(IEnumerable<JobPostingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetJobPostingsBySkill(int skillId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var jobPostings = await _jobPostingService.GetJobPostingsBySkillAsync(skillId, pageNumber, pageSize);
            return Ok(jobPostings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request getting job postings by skill");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job postings by skill");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get job postings by employment type
    /// </summary>
    [HttpGet("type/{employmentType}")]
    [ProducesResponseType(typeof(IEnumerable<JobPostingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetJobPostingsByType(string employmentType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var jobPostings = await _jobPostingService.GetJobPostingsByTypeAsync(employmentType, pageNumber, pageSize);
            return Ok(jobPostings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request getting job postings by type");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job postings by type");
            return StatusCode(500, "Internal server error");
        }
    }
}

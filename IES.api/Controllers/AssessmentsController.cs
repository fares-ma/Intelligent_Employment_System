using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Assessment;
using System.Security.Claims;

namespace IES.api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService _assessmentService;
    private readonly ILogger<AssessmentsController> _logger;

    public AssessmentsController(IAssessmentService assessmentService, ILogger<AssessmentsController> logger)
    {
        _assessmentService = assessmentService ?? throw new ArgumentNullException(nameof(assessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new assessment for a job.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssessmentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var assessment = await _assessmentService.CreateAssessmentAsync(userId, request);
            return CreatedAtAction(nameof(GetAssessment), new { assessmentId = assessment.Id }, assessment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assessment");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get assessment details.
    /// </summary>
    [HttpGet("{assessmentId}")]
    [ProducesResponseType(typeof(AssessmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssessment(int assessmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var assessment = await _assessmentService.GetAssessmentAsync(assessmentId, userId);
            
            return Ok(assessment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Update an assessment.
    /// </summary>
    [HttpPut("{assessmentId}")]
    [ProducesResponseType(typeof(AssessmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAssessment(int assessmentId, [FromBody] UpdateAssessmentDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var assessment = await _assessmentService.UpdateAssessmentAsync(assessmentId, userId, request);
            return Ok(assessment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete an assessment.
    /// </summary>
    [HttpDelete("{assessmentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAssessment(int assessmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _assessmentService.DeleteAssessmentAsync(assessmentId, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// List assessments for a job.
    /// </summary>
    [HttpGet("job/{jobPostId}")]
    [ProducesResponseType(typeof(IEnumerable<AssessmentListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJobAssessments(int jobPostId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var assessments = await _assessmentService.GetJobAssessmentsAsync(jobPostId, userId);
            return Ok(assessments);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Start an assessment attempt (candidate).
    /// </summary>
    [HttpPost("{assessmentId}/start")]
    [ProducesResponseType(typeof(CandidateAssessmentAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartAssessment(int assessmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var attempt = await _assessmentService.StartAssessmentAsync(assessmentId, userId);
            return Ok(attempt);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Submit assessment answers (candidate).
    /// </summary>
    [HttpPost("{assessmentId}/submit")]
    [ProducesResponseType(typeof(SubmitAssessmentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitAssessment(int assessmentId, [FromBody] SubmitAssessmentDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _assessmentService.SubmitAssessmentAsync(assessmentId, userId, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get assessment results for all candidates (recruiter).
    /// </summary>
    [HttpGet("{assessmentId}/results")]
    [ProducesResponseType(typeof(Shared.Pagination.PagedResult<CandidateAssessmentResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssessmentResults(int assessmentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var results = await _assessmentService.GetAssessmentResultsAsync(assessmentId, userId, pageNumber, pageSize);
            return Ok(results);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

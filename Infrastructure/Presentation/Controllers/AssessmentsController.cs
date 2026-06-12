using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.Assessment;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssessmentsController : ControllerBase
{
    private readonly IAssessmentService _assessmentService;

    public AssessmentsController(IAssessmentService assessmentService)
    {
        _assessmentService = assessmentService;
    }

    // --- RECRUITER / ADMIN ENDPOINTS ---

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(AssessmentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AssessmentDto>> CreateAssessment([FromBody] CreateAssessmentDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _assessmentService.CreateAssessmentAsync(request, userId);
        return CreatedAtAction(nameof(GetAssessment), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(AssessmentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssessmentDto>> GetAssessment(int id)
    {
        var result = await _assessmentService.GetAssessmentAsync(id);
        return Ok(result);
    }

    [HttpGet("job/{jobPostId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<AssessmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessmentsForJob(int jobPostId)
    {
        var result = await _assessmentService.GetAssessmentsForJobAsync(jobPostId);
        return Ok(result);
    }

    [HttpGet("{id}/candidates")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(Shared.Pagination.PagedResult<AssessmentCandidateListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Shared.Pagination.PagedResult<AssessmentCandidateListDto>>> GetAssessmentCandidates(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var pagination = new Shared.Pagination.PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _assessmentService.GetAssessmentCandidatesAsync(id, pagination);
        return Ok(result);
    }

    [HttpGet("{id}/candidates/{candidateId}")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(AssessmentCandidateDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssessmentCandidateDetailDto>> GetCandidateAssessmentDetails(int id, string candidateId)
    {
        var result = await _assessmentService.GetCandidateAssessmentDetailsAsync(id, candidateId);
        return Ok(result);
    }

    // --- CANDIDATE ENDPOINTS ---

    [HttpPost("{id}/start")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(CandidateAssessmentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CandidateAssessmentDto>> StartAssessment(int id, [FromQuery] int jobApplicationId)
    {
        try
        {
            var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var result = await _assessmentService.StartAssessmentAsync(id, jobApplicationId, candidateId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Domain.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex) when (ex is ArgumentException || ex is Domain.Exceptions.BadRequestException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(CandidateAssessmentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CandidateAssessmentDto>> SubmitAssessment(int id, [FromBody] SubmitAssessmentDto request)
    {
        if (id != request.AssessmentId) return BadRequest("ID mismatch");

        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _assessmentService.SubmitAssessmentAsync(request, candidateId);
        return Ok(result);
    }

    [HttpGet("my-assessments")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(IEnumerable<CandidateAssessmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CandidateAssessmentDto>>> GetMyAssessments()
    {
        var candidateId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _assessmentService.GetCandidateAssessmentsAsync(candidateId);
        return Ok(result);
    }
}

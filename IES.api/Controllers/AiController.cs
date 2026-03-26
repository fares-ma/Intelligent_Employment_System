using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using Services.Abstractions.DTOs.AI;
using System.Security.Claims;

namespace IES.api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AiController : ControllerBase
{
    private readonly IAiServiceClient _aiServiceClient;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiServiceClient aiServiceClient, ILogger<AiController> logger)
    {
        _aiServiceClient = aiServiceClient ?? throw new ArgumentNullException(nameof(aiServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Extract structured skills from a free-text job description
    /// </summary>
    [HttpPost("extract-skills")]
    [ProducesResponseType(typeof(ExtractSkillsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ExtractSkills([FromBody] ExtractSkillsRequestDto request)
    {
        try
        {
            var skills = await _aiServiceClient.ExtractSkillsAsync(request.Text);
            
            // Temporary stub mapping
            var response = new ExtractSkillsResponseDto
            {
                Skills = skills.Select(s => new ExtractedSkillDto { Name = s, IsRequired = true }).ToList(),
                Summary = "Skill extraction requires full AI integration."
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting skills");
            return StatusCode(503, "AI service is temporarily unavailable. Please try again later.");
        }
    }

    /// <summary>
    /// Analyze a resume against job requirements
    /// </summary>
    [HttpPost("analyze-resume")]
    [ProducesResponseType(typeof(AnalyzeResumeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AnalyzeResume([FromBody] AnalyzeResumeRequestDto request)
    {
        try
        {
            // Stub reading from AI client
            var (score, report) = await _aiServiceClient.ScoreResumeAsync("Stub resume text", "Stub job desc");
            
            return Ok(new AnalyzeResumeResponseDto
            {
                MatchScore = score,
                MatchReport = report,
                SkillsGap = new List<SkillGapDto>() // Stub
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing resume {ResumeId} for Job {JobPostId}", request.ResumeId, request.JobPostId);
            return StatusCode(503, "AI service is temporarily unavailable. Please try again later.");
        }
    }

    /// <summary>
    /// Generate an AI-enhanced CV for a candidate
    /// </summary>
    [HttpPost("generate-cv")]
    [ProducesResponseType(typeof(GenerateCvResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GenerateCv([FromBody] GenerateCvRequestDto request)
    {
        try
        {
            var cvPath = await _aiServiceClient.GenerateCvAsync("Stub resume text", "Candidate Placeholder");
            return Ok(new GenerateCvResponseDto
            {
                CvPath = cvPath,
                Summary = "CV generated successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CV for resume {ResumeId}", request.ResumeId);
            return StatusCode(503, "AI service is temporarily unavailable. Please try again later.");
        }
    }
    
    // Additional endpoints for Assessment and Interviews (T111/T112 scope)
    [HttpPost("generate-assessment")]
    public async Task<IActionResult> GenerateAssessment([FromBody] GenerateAssessmentRequestDto request)
    {
        try
        {
            await _aiServiceClient.GenerateAssessmentAsync("Stub job desc", request.QuestionCount ?? 10);
            return Ok(new { questions = new List<object>() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating assessment");
            return StatusCode(503, "AI service is temporarily unavailable. Please try again later.");
        }
    }
}

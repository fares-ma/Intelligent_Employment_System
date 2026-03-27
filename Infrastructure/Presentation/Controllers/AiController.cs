using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.AI;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AiController : ControllerBase
{
    private readonly IAiServiceClient _aiServiceClient;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiServiceClient aiServiceClient, ILogger<AiController> logger)
    {
        _aiServiceClient = aiServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Extract skills from a job description
    /// </summary>
    /// <response code="200">Skills extracted successfully</response>
    /// <response code="503">AI Service is unavailable</response>
    [HttpPost("extract-skills")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<List<string>>> ExtractSkills([FromBody] ExtractSkillsRequestDto request)
    {
        _logger.LogInformation("Extracting skills from job description");
        var skills = await _aiServiceClient.ExtractSkillsAsync(request.JobDescription);
        return Ok(skills);
    }

    /// <summary>
    /// Analyze a candidate's resume against a job description
    /// </summary>
    /// <response code="200">Analysis completed successfully</response>
    /// <response code="503">AI Service is unavailable</response>
    [HttpPost("analyze-resume")]
    [Authorize(Roles = "Admin,Recruiter,Candidate")]
    [ProducesResponseType(typeof(AnalyzeResumeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AnalyzeResumeResponseDto>> AnalyzeResume([FromBody] AnalyzeResumeRequestDto request)
    {
        _logger.LogInformation("Analyzing resume against job description");
        var (score, report) = await _aiServiceClient.ScoreResumeAsync(request.ResumeText, request.JobDescription);
        
        return Ok(new AnalyzeResumeResponseDto 
        { 
            Score = score, 
            Report = report 
        });
    }

    /// <summary>
    /// Generate an optimized CV for a candidate based on their profile data
    /// </summary>
    /// <response code="200">CV generated successfully</response>
    /// <response code="503">AI Service is unavailable</response>
    [HttpPost("generate-cv")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<string>> GenerateCv([FromBody] GenerateCvRequestDto request)
    {
        _logger.LogInformation("Generating CV for candidate: {CandidateName}", request.CandidateName);
        var cvText = await _aiServiceClient.GenerateCvAsync(request.ResumeText, request.CandidateName);
        return Ok(cvText);
    }

    /// <summary>
    /// Generate assessment questions based on a job summary
    /// </summary>
    /// <response code="200">Assessment questions generated successfully</response>
    /// <response code="503">AI Service is unavailable</response>
    [HttpPost("generate-assessment")]
    [Authorize(Roles = "Admin,Recruiter")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<List<string>>> GenerateAssessment([FromBody] GenerateAssessmentRequestDto request)
    {
        _logger.LogInformation("Generating {Count} assessment questions from job description", request.QuestionCount);
        var questions = await _aiServiceClient.GenerateAssessmentAsync(request.JobDescription, request.QuestionCount);
        return Ok(questions);
    }
}

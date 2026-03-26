using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstractions;
using System.Security.Claims;

namespace IES.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiServiceClient _aiClient;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiServiceClient aiClient, ILogger<AiController> logger)
    {
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("extract-skills")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> ExtractSkills([FromBody] ExtractSkillsRequest request)
    {
        try
        {
            var skills = await _aiClient.ExtractSkillsAsync(request.JobDescription);
            return Ok(new { skills });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting skills");
            return StatusCode(503, new { message = "AI service unavailable" });
        }
    }

    [HttpPost("analyze-resume")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> AnalyzeResume([FromBody] AnalyzeResumeRequest request)
    {
        try
        {
            var (score, report) = await _aiClient.ScoreResumeAsync(request.ResumeText, request.JobDescription);
            return Ok(new { score, report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing resume");
            return StatusCode(503, new { message = "AI service unavailable" });
        }
    }

    [HttpPost("generate-cv")]
    public async Task<IActionResult> GenerateCv([FromBody] GenerateCvRequest request)
    {
        try
        {
            var cvPath = await _aiClient.GenerateCvAsync(request.ResumeText, request.CandidateName);
            return Ok(new { cvPath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CV");
            return StatusCode(503, new { message = "AI service unavailable" });
        }
    }

    [HttpPost("generate-interview-questions")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> GenerateInterviewQuestions([FromBody] GenerateInterviewQuestionsRequest request)
    {
        try
        {
            var questions = await _aiClient.GenerateInterviewQuestionsAsync(request.JobDescription, request.CandidateSkills);
            return Ok(new { questions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating interview questions");
            return StatusCode(503, new { message = "AI service unavailable" });
        }
    }

    [HttpPost("score-interview")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> ScoreInterview([FromBody] ScoreInterviewRequest request)
    {
        try
        {
            var score = await _aiClient.ScoreInterviewAsync(request.Question, request.Response, request.ExpectedAnswer);
            return Ok(new { score });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scoring interview");
            return StatusCode(503, new { message = "AI service unavailable" });
        }
    }
}

public class ExtractSkillsRequest
{
    public string JobDescription { get; set; } = string.Empty;
}

public class AnalyzeResumeRequest
{
    public string ResumeText { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
}

public class GenerateCvRequest
{
    public string ResumeText { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
}

public class GenerateInterviewQuestionsRequest
{
    public string JobDescription { get; set; } = string.Empty;
    public List<string> CandidateSkills { get; set; } = new();
}

public class ScoreInterviewRequest
{
    public string Question { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string ExpectedAnswer { get; set; } = string.Empty;
}

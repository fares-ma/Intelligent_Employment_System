using Services.Abstractions;

namespace Services;

public class AiServiceClient : IAiServiceClient
{
    public Task<(decimal Score, string Report)> ScoreResumeAsync(string resumeText, string jobDescription)
    {
        // TODO: Implement when AI service is ready
        return Task.FromResult((100m, "AI service not yet integrated"));
    }

    public Task<List<string>> ExtractSkillsAsync(string jobDescription)
    {
        // TODO: Implement when AI service is ready
        return Task.FromResult(new List<string>());
    }

    public Task<List<string>> GenerateAssessmentAsync(string jobDescription, int questionCount = 5)
    {
        // TODO: Implement when AI service is ready
        return Task.FromResult(new List<string>());
    }

    public Task<string> GenerateCvAsync(string resumeText, string candidateName)
    {
        // TODO: Implement when AI service is ready
        return Task.FromResult($"cv_{Guid.NewGuid()}.pdf");
    }

    public Task<List<string>> GenerateInterviewQuestionsAsync(string jobDescription, List<string> candidateSkills)
    {
        // TODO: Implement when AI service is ready
        return Task.FromResult(new List<string>());
    }

    public Task<decimal> ScoreInterviewAsync(string question, string response, string expectedAnswer)
    {
        // TODO: Implement when AI service is ready
        return Task.FromResult(0m);
    }
}

namespace Services.Abstractions;

public interface IAiServiceClient
{
    /// <summary>
    /// Score a resume against a job description (0-100)
    /// </summary>
    Task<(decimal Score, string Report)> ScoreResumeAsync(string resumeText, string jobDescription);

    /// <summary>
    /// Extract skills from job description
    /// </summary>
    Task<List<string>> ExtractSkillsAsync(string jobDescription);

    /// <summary>
    /// Generate assessment questions for a job
    /// </summary>
    Task<List<string>> GenerateAssessmentAsync(string jobDescription, int questionCount = 5);

    /// <summary>
    /// Generate CV from resume and profile
    /// </summary>
    Task<string> GenerateCvAsync(string resumeText, string candidateName);

    /// <summary>
    /// Generate interview questions for a candidate
    /// </summary>
    Task<List<string>> GenerateInterviewQuestionsAsync(string jobDescription, List<string> candidateSkills);

    /// <summary>
    /// Score an interview response
    /// </summary>
    Task<decimal> ScoreInterviewAsync(string question, string response, string expectedAnswer);
}

using System.ComponentModel.DataAnnotations;

namespace Services.Abstractions.DTOs.AI;

public class ExtractSkillsRequestDto
{
    [Required]
    [MaxLength(5000)]
    public string Text { get; set; } = string.Empty;
}

public class ExtractSkillsResponseDto
{
    public List<ExtractedSkillDto> Skills { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class ExtractedSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsRequired { get; set; }
}

public class AnalyzeResumeRequestDto
{
    [Required]
    public int ResumeId { get; set; }
    [Required]
    public int JobPostId { get; set; }
}

public class AnalyzeResumeResponseDto
{
    public decimal MatchScore { get; set; }
    public string MatchReport { get; set; } = string.Empty;
    public List<SkillGapDto> SkillsGap { get; set; } = new();
}

public class SkillGapDto
{
    public string Skill { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // e.g. "Missing", "Met"
}

public class GenerateAssessmentRequestDto
{
    [Required]
    public int JobPostId { get; set; }
    public int? QuestionCount { get; set; } = 10;
    public List<int>? QuestionTypes { get; set; }
}

public class GenerateCvRequestDto
{
    [Required]
    public int ResumeId { get; set; }
}

public class GenerateCvResponseDto
{
    public string CvPath { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public class GenerateInterviewQuestionsRequestDto
{
    [Required]
    public int JobPostId { get; set; }
    public int? QuestionCount { get; set; } = 5;
}

public class ScoreInterviewRequestDto
{
    [Required]
    public int InterviewId { get; set; }
    [Required]
    public string Transcript { get; set; } = string.Empty;
    public List<InterviewQuestionTopicDto> Questions { get; set; } = new();
}

public class InterviewQuestionTopicDto
{
    public string Text { get; set; } = string.Empty;
    public List<string> ExpectedTopics { get; set; } = new();
}

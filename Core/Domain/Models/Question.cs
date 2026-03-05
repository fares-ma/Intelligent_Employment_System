using Domain.Enums;

namespace Domain.Models;

/// <summary>
/// Individual assessment question — MCQ, TrueFalse, OpenEnded, or Coding.
/// </summary>
public class Question
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    /// <summary>
    /// JSON array for MCQ choices.
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// For auto-grading MCQ/TrueFalse.
    /// </summary>
    public string? CorrectAnswer { get; set; }

    public int Points { get; set; }

    public int OrderIndex { get; set; }
}

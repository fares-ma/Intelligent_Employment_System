namespace Domain.Enums;

/// <summary>
/// Tracks async TalentX resume scoring lifecycle for a job application.
/// MatchScore remains null while status is Processing (per three-state MatchScore spec).
/// </summary>
public enum AiScoringStatus
{
    NotStarted = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

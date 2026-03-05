namespace Domain.Enums;

/// <summary>
/// Strict sequential pipeline for job applications.
/// </summary>
public enum ApplicationStatus
{
    Pending = 0,
    UnderReview = 1,
    Assessment = 2,
    Interview = 3,
    Accepted = 4,
    Rejected = 5
}

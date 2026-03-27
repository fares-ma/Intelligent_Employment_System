namespace Services.Abstractions.DTOs.Dashboard;

public class CandidateDashboardDto
{
    public int SavedJobsCount { get; set; }
    public int ActiveApplicationsCount { get; set; }
    public int UpcomingInterviewsCount { get; set; }
    public int UnreadNotificationsCount { get; set; }
}

public class CompanyDashboardDto
{
    public int ActiveJobsCount { get; set; }
    public int TotalApplicantsCount { get; set; }
    public int PendingInterviewsCount { get; set; }
    public int UnreadNotificationsCount { get; set; }
}

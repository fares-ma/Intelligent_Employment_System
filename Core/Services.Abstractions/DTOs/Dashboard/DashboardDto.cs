namespace Services.Abstractions.DTOs.Dashboard;

public class CandidateDashboardDto
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int UnderReviewApplications { get; set; }
    public int InterviewApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int SavedJobs { get; set; }
    public int TotalResumes { get; set; }
}

public class CompanyDashboardDto
{
    public int TotalJobPostings { get; set; }
    public int ActiveJobPostings { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int InterviewApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int TotalRecruiters { get; set; }
}

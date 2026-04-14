using Services.Abstractions.DTOs.Interview;
using Services.Abstractions.DTOs.JobApplication;
using Services.Abstractions.DTOs.JobPosting;

namespace Services.Abstractions.DTOs.Dashboard;

public class CandidateDashboardDto
{
    public int SavedJobsCount { get; set; }
    public int ActiveApplicationsCount { get; set; }
    public int UpcomingInterviewsCount { get; set; }
    public int UnreadNotificationsCount { get; set; }
    
    public IEnumerable<JobApplicationDto> RecentApplications { get; set; } = new List<JobApplicationDto>();
    public IEnumerable<InterviewDto> UpcomingInterviews { get; set; } = new List<InterviewDto>();
}

public class CompanyDashboardDto
{
    public int ActiveJobsCount { get; set; }
    public int TotalApplicantsCount { get; set; }
    public int PendingInterviewsCount { get; set; }
    public int UnreadNotificationsCount { get; set; }

    public IEnumerable<JobPostingDto> RecentJobPosts { get; set; } = new List<JobPostingDto>();
    public IEnumerable<JobApplicationDto> RecentApplicants { get; set; } = new List<JobApplicationDto>();
}

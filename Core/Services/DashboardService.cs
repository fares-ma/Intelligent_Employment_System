using Domain.Contracts;
using Domain.Exceptions;

using Services.Abstractions;
using Services.Abstractions.DTOs.Dashboard;

namespace Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CandidateDashboardDto> GetCandidateDashboardAsync(string candidateId)
    {
        var savedJobsQuery = await _unitOfWork.SavedJobs.FindAsync(sj => sj.CandidateId == candidateId);
        var savedJobsCount = savedJobsQuery.Count();

        var activeAppsQuery = await _unitOfWork.JobApplications.FindAsync(ja => ja.CandidateId == candidateId && ja.Status != Domain.Enums.ApplicationStatus.Withdrawn && ja.Status != Domain.Enums.ApplicationStatus.Rejected);
        var activeApplicationsCount = activeAppsQuery.Count();

        var upcomingInterviewsQuery = await _unitOfWork.Interviews.FindAsync(i => i.JobApplication.CandidateId == candidateId && i.Status == Domain.Enums.InterviewStatus.Scheduled);
        var upcomingInterviewsCount = upcomingInterviewsQuery.Count();

        var unreadNotificationsCount = await _unitOfWork.Notifications.GetUnreadCountAsync(candidateId);

        return new CandidateDashboardDto
        {
            SavedJobsCount = savedJobsCount,
            ActiveApplicationsCount = activeApplicationsCount,
            UpcomingInterviewsCount = upcomingInterviewsCount,
            UnreadNotificationsCount = unreadNotificationsCount
        };
    }

    public async Task<CompanyDashboardDto> GetCompanyDashboardAsync(int companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null) throw new NotFoundException("Company not found");

        var activeJobsQuery = await _unitOfWork.JobPosts.FindAsync(jp => jp.CompanyId == companyId && jp.IsActive && jp.IsPublished);
        var activeJobsCount = activeJobsQuery.Count();

        var applicantsQuery = await _unitOfWork.JobApplications.FindAsync(ja => ja.JobPost.CompanyId == companyId);
        var totalApplicantsCount = applicantsQuery.Count();

        var interviewsQuery = await _unitOfWork.Interviews.FindAsync(i => i.JobApplication.JobPost.CompanyId == companyId && i.Status == Domain.Enums.InterviewStatus.Scheduled);
        var pendingInterviewsCount = interviewsQuery.Count();

        // Company admins or recruiters might get notifications too, but let's just leave it at 0 for now
        // if we are not passing a specific recruiter userId.
        int unreadNotificationsCount = 0;

        return new CompanyDashboardDto
        {
            ActiveJobsCount = activeJobsCount,
            TotalApplicantsCount = totalApplicantsCount,
            PendingInterviewsCount = pendingInterviewsCount,
            UnreadNotificationsCount = unreadNotificationsCount
        };
    }
}

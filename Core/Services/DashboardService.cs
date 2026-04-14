using AutoMapper;
using Domain.Contracts;
using Domain.Exceptions;
using Services.Abstractions;
using Services.Abstractions.DTOs.Dashboard;
using Services.Abstractions.DTOs.Interview;
using Services.Abstractions.DTOs.JobApplication;
using Services.Abstractions.DTOs.JobPosting;
using Shared.Pagination;

namespace Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DashboardService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CandidateDashboardDto> GetCandidateDashboardAsync(string candidateId)
    {
        var savedJobsCount = await _unitOfWork.SavedJobs.CountAsync(sj => sj.CandidateId == candidateId);

        var activeApplicationsCount = await _unitOfWork.JobApplications.CountAsync(ja => ja.CandidateId == candidateId && ja.Status != Domain.Enums.ApplicationStatus.Withdrawn && ja.Status != Domain.Enums.ApplicationStatus.Rejected);

        var upcomingInterviewsCount = await _unitOfWork.Interviews.CountAsync(i => i.JobApplication.CandidateId == candidateId && i.Status == Domain.Enums.InterviewStatus.Scheduled);

        var unreadNotificationsCount = await _unitOfWork.Notifications.GetUnreadCountAsync(candidateId);

        // Recent Applications (Last 5)
        var recentAppsPaged = await _unitOfWork.JobApplications.GetByCandidateAsync(candidateId, new PaginationParams { PageNumber = 1, PageSize = 5 });
        var recentApps = _mapper.Map<IEnumerable<JobApplicationDto>>(recentAppsPaged.Items);

        // Upcoming Interviews
        var interviews = await _unitOfWork.Interviews.GetUpcomingAsync(candidateId);
        var upcomingInterviews = _mapper.Map<IEnumerable<InterviewDto>>(interviews.Take(5));

        return new CandidateDashboardDto
        {
            SavedJobsCount = savedJobsCount,
            ActiveApplicationsCount = activeApplicationsCount,
            UpcomingInterviewsCount = upcomingInterviewsCount,
            UnreadNotificationsCount = unreadNotificationsCount,
            RecentApplications = recentApps,
            UpcomingInterviews = upcomingInterviews
        };
    }

    public async Task<CompanyDashboardDto> GetCompanyDashboardAsync(int companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null) throw new NotFoundException("Company not found");

        var activeJobsCount = await _unitOfWork.JobPosts.CountAsync(jp => jp.CompanyId == companyId && jp.IsActive && jp.IsPublished);

        var totalApplicantsCount = await _unitOfWork.JobApplications.CountAsync(ja => ja.JobPost.CompanyId == companyId);

        var pendingInterviewsCount = await _unitOfWork.Interviews.CountAsync(i => i.JobApplication.JobPost.CompanyId == companyId && i.Status == Domain.Enums.InterviewStatus.Scheduled);

        // Get Recent Job Posts (Last 5)
        var recentJobsPaged = await _unitOfWork.JobPosts.GetCompanyJobsAsync(companyId, new PaginationParams { PageNumber = 1, PageSize = 5 });
        var recentJobs = _mapper.Map<IEnumerable<JobPostingDto>>(recentJobsPaged.Items);

        // Get Recent Applicants across all company jobs
        // This might need a custom repo method for "GetRecentApplicantsForCompanyAsync" if not available
        // For now, let's try to get them from FindAsync but it won't be as clean without order/include
        // Let's assume we fetch them and map them.
        
        var unreadNotificationsCount = 0;

        return new CompanyDashboardDto
        {
            ActiveJobsCount = activeJobsCount,
            TotalApplicantsCount = totalApplicantsCount,
            PendingInterviewsCount = pendingInterviewsCount,
            UnreadNotificationsCount = unreadNotificationsCount,
            RecentJobPosts = recentJobs,
            RecentApplicants = new List<JobApplicationDto>() // Future: add repo method
        };
    }
}

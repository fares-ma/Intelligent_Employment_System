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
        var recentApps = recentAppsPaged.Items.Select(ja => new JobApplicationDto
        {
            Id = ja.Id,
            CandidateId = ja.CandidateId,
            CandidateName = ja.Candidate?.UserName ?? "Unknown",
            JobPostId = ja.JobPostId,
            JobTitle = ja.JobPost?.Title ?? "Unknown Job",
            Status = ja.Status.ToString(),
            AppliedAt = ja.AppliedAt,
            UpdatedAt = ja.UpdatedAt,
            RejectionReason = ja.RecruiterNotes,
            MatchScore = ja.MatchScore,
            FitStatus = ja.FitStatus?.ToString(),
            AiScoringStatus = ja.AiScoringStatus.ToString(),
            RecruiterRating = ja.RecruiterRating
        });

        // Upcoming Interviews
        var interviews = await _unitOfWork.Interviews.GetUpcomingAsync(candidateId);
        var upcomingInterviews = interviews.Take(5).Select(i => new InterviewDto
        {
            Id = i.Id,
            JobApplicationId = i.JobApplicationId,
            CandidateId = i.JobApplication?.CandidateId ?? string.Empty,
            CandidateName = i.JobApplication?.Candidate?.UserName ?? "Unknown",
            JobTitle = i.JobApplication?.JobPost?.Title ?? "Unknown Job",
            InterviewType = i.InterviewType.ToString(),
            Status = i.Status.ToString(),
            ScheduledAt = i.ScheduledAt,
            DurationMinutes = i.DurationMinutes,
            MeetingLink = i.MeetingLink,
            Score = i.Score,
            CompletedAt = i.CompletedAt,
            CreatedAt = i.CreatedAt
        });

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
        var recentJobs = recentJobsPaged.Items.Select(jp => new JobPostingDto
        {
            Id = jp.Id,
            Title = jp.Title,
            Description = jp.Description,
            Requirements = "",
            Location = jp.Location ?? string.Empty,
            EmploymentType = jp.JobType.ToString(),
            CompanyId = jp.CompanyId,
            CompanyName = company.Name,
            IsActive = jp.IsActive,
            ApplicationDeadline = jp.ExpiryDate,
            CreatedAt = jp.CreatedAt,
            UpdatedAt = jp.UpdatedAt ?? jp.CreatedAt,
            ApplicationCount = jp.JobApplications?.Count ?? 0,
            Department = jp.Department,
            GPA = jp.GPA,
            GPAPriority = jp.GPAPriority,
            ExperienceMinYears = jp.ExperienceMinYears,
            ExperienceMaxYears = jp.ExperienceMaxYears,
            ExperiencePriority = jp.ExperiencePriority,
            RequiredSkills = jp.JobPostSkills?.Select(jps => jps.Skill?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>()
        });

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

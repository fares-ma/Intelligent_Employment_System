using Domain.Contracts;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Dashboard;

namespace Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CandidateDashboardDto> GetCandidateDashboardAsync(string candidateId)
    {
        _logger.LogInformation("Getting dashboard for candidate {CandidateId}", candidateId);

        var applications = await _unitOfWork.JobApplications.FindAsync(a => a.CandidateId == candidateId);
        var savedJobs = await _unitOfWork.SavedJobs.FindAsync(sj => sj.CandidateId == candidateId);
        var resumes = await _unitOfWork.Resumes.FindAsync(r => r.CandidateId == candidateId);

        return new CandidateDashboardDto
        {
            TotalApplications = applications.Count(),
            PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
            UnderReviewApplications = applications.Count(a => a.Status == ApplicationStatus.UnderReview),
            InterviewApplications = applications.Count(a => a.Status == ApplicationStatus.Interview),
            AcceptedApplications = applications.Count(a => a.Status == ApplicationStatus.Accepted),
            RejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Rejected),
            SavedJobs = savedJobs.Count(),
            TotalResumes = resumes.Count()
        };
    }

    public async Task<CompanyDashboardDto> GetCompanyDashboardAsync(string companyId)
    {
        _logger.LogInformation("Getting dashboard for company {CompanyId}", companyId);

        if (!int.TryParse(companyId, out var companyIdInt))
            throw new ArgumentException("Invalid company ID");

        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company is null)
            throw new ArgumentException("Company not found");

        var jobPostings = await _unitOfWork.JobPosts.FindAsync(jp => jp.CompanyId == companyIdInt);
        var recruiters = await _unitOfWork.Recruiters.FindAsync(r => r.CompanyId == companyIdInt);

        var jobPostIds = jobPostings.Select(jp => jp.Id).ToList();
        var applications = await _unitOfWork.JobApplications.FindAsync(a => jobPostIds.Contains(a.JobPostId));

        return new CompanyDashboardDto
        {
            TotalJobPostings = jobPostings.Count(),
            ActiveJobPostings = jobPostings.Count(jp => jp.IsActive),
            TotalApplications = applications.Count(),
            PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
            InterviewApplications = applications.Count(a => a.Status == ApplicationStatus.Interview),
            AcceptedApplications = applications.Count(a => a.Status == ApplicationStatus.Accepted),
            RejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Rejected),
            TotalRecruiters = recruiters.Count()
        };
    }
}

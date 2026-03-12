using Domain.Contracts;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.JobApplication;
using Shared.Pagination;

namespace Services;

/// <summary>
/// Service for managing job applications
/// </summary>
public class JobApplicationService : IJobApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JobApplicationService> _logger;

    public JobApplicationService(IUnitOfWork unitOfWork, ILogger<JobApplicationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JobApplicationDto> ApplyForJobAsync(string candidateId, CreateJobApplicationDto request)
    {
        _logger.LogInformation("Processing job application from candidate {CandidateId} for job {JobPostId}", candidateId, request.JobPostId);

        ValidateCreateApplicationInput(request);

        // Verify candidate exists
        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new ArgumentException("Candidate not found");

        // Verify job posting exists
        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(request.JobPostId);
        if (jobPost is null)
            throw new ArgumentException("Job posting not found");

        if (!jobPost.IsActive)
            throw new ArgumentException("This job posting is no longer active");

        // Check if candidate already applied
        var alreadyApplied = await _unitOfWork.JobApplications.ExistsAsync(candidateId, request.JobPostId);
        if (alreadyApplied)
            throw new ArgumentException("You have already applied for this job posting");

        // Verify resume exists and belongs to candidate
        var resume = await _unitOfWork.Resumes.GetByIdAsync(request.ResumeId);
        if (resume is null || resume.CandidateId != candidateId)
            throw new ArgumentException("Resume not found or doesn't belong to you");

        var application = new JobApplication
        {
            CandidateId = candidateId,
            JobPostId = request.JobPostId,
            ResumeId = request.ResumeId,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };

        _unitOfWork.JobApplications.Create(application);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Job application created with ID {ApplicationId}", application.Id);
        return MapToDto(application, candidate, jobPost);
    }

    public async Task<JobApplicationDto> GetApplicationAsync(int applicationId)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId);
        if (application is null)
            throw new ArgumentException("Application not found");

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(application.CandidateId);
        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);

        return MapToDto(application, candidate, jobPost);
    }

    public async Task<IEnumerable<JobApplicationDto>> GetCandidateApplicationsAsync(string candidateId, int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Retrieving applications for candidate {CandidateId}", candidateId);
        ValidatePagination(pageNumber, pageSize);

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new ArgumentException("Candidate not found");

        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _unitOfWork.JobApplications.GetByCandidateAsync(candidateId, pagination);

        var applications = new List<JobApplicationDto>();
        foreach (var app in result.Items)
        {
            var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(app.JobPostId);
            applications.Add(MapToDto(app, candidate, jobPost));
        }

        return applications;
    }

    public async Task<IEnumerable<JobApplicationDto>> GetJobApplicationsAsync(int jobPostId, int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Retrieving applications for job {JobPostId}", jobPostId);
        ValidatePagination(pageNumber, pageSize);

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(jobPostId);
        if (jobPost is null)
            throw new ArgumentException("Job posting not found");

        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _unitOfWork.JobApplications.GetByJobPostAsync(jobPostId, pagination);

        var applications = new List<JobApplicationDto>();
        foreach (var app in result.Items)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(app.CandidateId);
            applications.Add(MapToDto(app, candidate, jobPost));
        }

        return applications;
    }

    public async Task<JobApplicationDto> UpdateApplicationStatusAsync(int applicationId, string recruiterId, UpdateJobApplicationDto request)
    {
        _logger.LogInformation("Updating application {ApplicationId} status", applicationId);
        ValidateUpdateApplicationInput(request);

        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId);
        if (application is null)
            throw new ArgumentException("Application not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);
        if (jobPost is null)
            throw new ArgumentException("Job posting not found");

        // Verify recruiter owns this job posting
        if (jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to update this application");

        // Parse status
        if (!Enum.TryParse<ApplicationStatus>(request.Status, true, out var status))
            throw new ArgumentException("Invalid status value");

        application.Status = status;
        application.UpdatedAt = DateTime.UtcNow;

        // Store recruiter notes
        if (request.RejectionReason != null)
        {
            application.RecruiterNotes = request.RejectionReason;
        }

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(application.CandidateId);
        return MapToDto(application, candidate, jobPost);
    }

    public async Task WithdrawApplicationAsync(int applicationId, string candidateId)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId);
        if (application is null)
            throw new ArgumentException("Application not found");

        if (application.CandidateId != candidateId)
            throw new UnauthorizedAccessException("You can only withdraw your own applications");

        // Only allow withdrawal if status is Pending
        if (application.Status != ApplicationStatus.Pending)
            throw new ArgumentException("Can only withdraw applications with Pending status");

        _unitOfWork.JobApplications.Delete(application);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Application {ApplicationId} withdrawn", applicationId);
    }

    public async Task<bool> HasAppliedAsync(string candidateId, int jobPostId)
    {
        return await _unitOfWork.JobApplications.ExistsAsync(candidateId, jobPostId);
    }

    public async Task<IEnumerable<JobApplicationDto>> GetApplicationsByStatusAsync(int jobPostId, string status, int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Retrieving {Status} applications for job {JobPostId}", status, jobPostId);
        ValidatePagination(pageNumber, pageSize);

        if (!Enum.TryParse<ApplicationStatus>(status, true, out var appStatus))
            throw new ArgumentException("Invalid status value");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(jobPostId);
        if (jobPost is null)
            throw new ArgumentException("Job posting not found");

        var applications = await _unitOfWork.JobApplications.FindAsync(
            app => app.JobPostId == jobPostId && app.Status == appStatus
        );

        var result = new List<JobApplicationDto>();
        foreach (var app in applications
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize))
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(app.CandidateId);
            result.Add(MapToDto(app, candidate, jobPost));
        }

        return result;
    }

    private static JobApplicationDto MapToDto(JobApplication application, CandidateUser? candidate, JobPost? jobPost)
    {
        return new JobApplicationDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            CandidateName = candidate?.UserName ?? string.Empty,
            JobPostId = application.JobPostId,
            JobTitle = jobPost?.Title ?? string.Empty,
            Status = application.Status.ToString(),
            CoverLetter = null,
            ResumeId = application.ResumeId > 0 ? application.ResumeId : null,
            AppliedAt = application.AppliedAt,
            UpdatedAt = application.UpdatedAt,
            RejectionReason = application.RecruiterNotes,
            MatchScore = application.MatchScore,
            RecruiterRating = application.RecruiterRating
        };
    }

    private static void ValidateCreateApplicationInput(CreateJobApplicationDto request)
    {
        if (request.JobPostId <= 0)
            throw new ArgumentException("Invalid job posting ID");

        if (request.ResumeId <= 0)
            throw new ArgumentException("Resume ID is required");
    }

    private static void ValidateUpdateApplicationInput(UpdateJobApplicationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            throw new ArgumentException("Status is required");

        var validStatuses = new[] { "Pending", "UnderReview", "Assessment", "Interview", "Accepted", "Rejected", "Withdrawn" };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException("Invalid status value");

        if (request.Status == "Rejected" && string.IsNullOrWhiteSpace(request.RejectionReason))
            throw new ArgumentException("Recruiter notes are required when rejecting an application");

        if (!string.IsNullOrWhiteSpace(request.RejectionReason) && request.RejectionReason.Length > 500)
            throw new ArgumentException("Recruiter notes must not exceed 500 characters");
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be at least 1");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100");
    }
}

using Domain.Contracts;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.JobApplication;
using Services.Abstractions.TalentX;
using Shared.Pagination;

namespace Services;

/// <summary>
/// Service for managing job applications
/// </summary>
public class JobApplicationService : IJobApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITalentXScoringService _talentXScoringService;
    private readonly ILogger<JobApplicationService> _logger;
    private readonly IEmailService _emailService;

    public JobApplicationService(
        IUnitOfWork unitOfWork,
        ITalentXScoringService talentXScoringService,
        ILogger<JobApplicationService> logger,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _talentXScoringService = talentXScoringService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService;
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

        if (!jobPost.IsActive || jobPost.IsDeleted || jobPost.Status != "ACTIVE")
            throw new ArgumentException("This job is no longer active and cannot accept applications");

        // Check if candidate already applied
        var alreadyApplied = await _unitOfWork.JobApplications.ExistsAsync(candidateId, request.JobPostId);
        if (alreadyApplied)
            throw new ConflictException("You have already applied for this job posting");

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

        // Async TalentX scoring (202 Accepted + webhook). Failures are logged; application stays with MatchScore=null.
        try
        {
            await _talentXScoringService.InitiateScoringForApplicationAsync(application.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to initiate TalentX scoring for application {ApplicationId}. Polling fallback may recover.",
                application.Id);
        }

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
            var jobPost = await _unitOfWork.JobPosts.GetByIdWithSkillsAsync(app.JobPostId);
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

    public async Task<JobApplicationDto> UpdateApplicationStatusAsync(int applicationId, string recruiterId, bool isAdmin, UpdateJobApplicationDto request)
    {
        _logger.LogInformation("Updating application {ApplicationId} status", applicationId);
        ValidateUpdateApplicationInput(request);

        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId);
        if (application is null)
            throw new ArgumentException("Application not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);
        if (jobPost is null)
            throw new ArgumentException("Job posting not found");

        // Removed recruiter ownership check per user request

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
        
        // Automated Emails
        if (candidate != null)
        {
            try
            {
                if (status == ApplicationStatus.Assessment)
                {
                    // Check if assessment exists
                    var assessment = await _unitOfWork.Assessments.GetByJobPostAsync(jobPost.Id);
                    if (assessment != null && assessment.Any())
                    {
                        var activeAssessment = assessment.FirstOrDefault(a => a.IsActive);
                        if (activeAssessment != null)
                        {
                            // Assign assessment to candidate if not already assigned
                            var existingCandidateAssessment = await _unitOfWork.CandidateAssessments.GetByCandidateAndAssessmentAsync(candidate.Id, activeAssessment.Id);
                            if (existingCandidateAssessment == null)
                            {
                                var newCandidateAssessment = new CandidateAssessment
                                {
                                    CandidateId = candidate.Id,
                                    AssessmentId = activeAssessment.Id,
                                    JobApplicationId = application.Id,
                                    IsCompleted = false
                                };
                                _unitOfWork.CandidateAssessments.Create(newCandidateAssessment);
                                await _unitOfWork.SaveChangesAsync();
                            }

                            var subject = $"Assessment Invitation: {jobPost.Title}";
                            var body = $"<h1>Assessment Invitation</h1><p>Dear {candidate.UserName},</p><p>You have been invited to take the assessment <strong>{activeAssessment.Title}</strong> for the position of {jobPost.Title}.</p>";
                            
                            if (activeAssessment.StartDate.HasValue)
                                body += $"<p><strong>Available From:</strong> {activeAssessment.StartDate.Value.ToString("g")}</p>";
                            if (activeAssessment.EndDate.HasValue)
                                body += $"<p><strong>Available Until:</strong> {activeAssessment.EndDate.Value.ToString("g")}</p>";
                            
                            body += $"<p><strong>Time Limit:</strong> {activeAssessment.TimeLimitMinutes} minutes</p>";
                            
                            if (!string.IsNullOrEmpty(activeAssessment.Instructions))
                                body += $"<p><strong>Instructions:</strong> {activeAssessment.Instructions}</p>";
                                
                            body += "<p>Good luck!</p>";
                            
                            await _emailService.SendEmailAsync(candidate.Email ?? "", subject, body);
                        }
                    }
                }
                else if (status == ApplicationStatus.Accepted)
                {
                    var subject = $"Congratulations! Job Offer for {jobPost.Title}";
                    var body = $"<h1>Congratulations!</h1><p>Dear {candidate.UserName},</p><p>We are thrilled to inform you that you have been accepted for the <strong>{jobPost.Title}</strong> position.</p><p>Our team will contact you shortly with the next steps.</p>";
                    await _emailService.SendEmailAsync(candidate.Email ?? "", subject, body);
                }
                else if (status == ApplicationStatus.Rejected)
                {
                    var subject = $"Update regarding your application for {jobPost.Title}";
                    var body = $"<h1>Application Update</h1><p>Dear {candidate.UserName},</p><p>Thank you for your interest in the <strong>{jobPost.Title}</strong> position.</p><p>We regret to inform you that we will not be moving forward with your application at this time.</p><p>We wish you the best in your future endeavors.</p>";
                    await _emailService.SendEmailAsync(candidate.Email ?? "", subject, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send automated email for application status update. Application ID: {AppId}, Status: {Status}", applicationId, status);
            }
        }

        return MapToDto(application, candidate, jobPost);
    }

    public async Task<JobApplicationDto> UpdateRecruiterRatingAsync(int applicationId, string recruiterId, bool isAdmin, int rating)
    {
        _logger.LogInformation("Updating rating for application {ApplicationId}", applicationId);

        if (rating < 1 || rating > 10)
            throw new ArgumentException("Rating must be between 1 and 10");

        var application = await _unitOfWork.JobApplications.GetByIdAsync(applicationId);
        if (application is null)
            throw new ArgumentException("Application not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);
        if (jobPost is null)
            throw new ArgumentException("Job posting not found");

        // Removed recruiter ownership check per user request

        application.RecruiterRating = rating;
        application.UpdatedAt = DateTime.UtcNow;

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
            CandidateName = candidate != null ? $"{candidate.FirstName} {candidate.LastName}".Trim() : string.Empty,
            CandidateEmail = candidate?.Email ?? string.Empty,
            CandidateProfilePictureUrl = !string.IsNullOrEmpty(candidate?.ProfilePicturePath)
                ? $"/api/Candidates/profile-picture/{System.IO.Path.GetFileName(candidate.ProfilePicturePath)}"
                : null,
            JobPostId = application.JobPostId,
            JobTitle = jobPost?.Title ?? string.Empty,
            CompanyName = jobPost?.Company?.Name ?? string.Empty,
            CompanyLogoUrl = jobPost?.Company?.LogoPath,
            Status = application.Status.ToString(),
            CoverLetter = null,
            ResumeId = application.ResumeId > 0 ? application.ResumeId : null,
            AppliedAt = application.AppliedAt,
            UpdatedAt = application.UpdatedAt,
            RejectionReason = application.RecruiterNotes,
            MatchScore = application.MatchScore,
            FitStatus = application.FitStatus,
            AiScoringStatus = application.AiScoringStatus.ToString(),
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

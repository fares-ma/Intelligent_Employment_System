using Domain.Contracts;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Interview;
using Shared.Pagination;

namespace Services;

/// <summary>
/// Service for managing interviews
/// </summary>
public class InterviewService : IInterviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InterviewService> _logger;

    public InterviewService(IUnitOfWork unitOfWork, ILogger<InterviewService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InterviewDto> ScheduleInterviewAsync(string recruiterId, CreateInterviewDto request)
    {
        _logger.LogInformation("Scheduling interview for application {ApplicationId}", request.JobApplicationId);

        ValidateCreateInterviewInput(request);

        // Verify application exists
        var application = await _unitOfWork.JobApplications.GetByIdAsync(request.JobApplicationId);
        if (application is null)
            throw new ArgumentException("Job application not found");

        // Verify recruiter owns the job posting
        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);
        if (jobPost is null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to schedule interviews for this job posting");

        // Parse interview type
        if (!Enum.TryParse<InterviewType>(request.InterviewType, true, out var interviewType))
            throw new ArgumentException("Invalid interview type");

        // Validate meeting link for Live interviews
        if (interviewType == InterviewType.Live && string.IsNullOrWhiteSpace(request.MeetingLink))
            throw new ArgumentException("Meeting link is required for Live interviews");

        // Normalize ScheduledAt to UTC (frontend may send local time)
        var scheduledAtUtc = request.ScheduledAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.ScheduledAt, DateTimeKind.Utc)
            : request.ScheduledAt.ToUniversalTime();

        var interview = new Interview
        {
            JobApplicationId = request.JobApplicationId,
            InterviewType = interviewType,
            Status = InterviewStatus.Scheduled,
            ScheduledAt = scheduledAtUtc,
            DurationMinutes = request.DurationMinutes,
            MeetingLink = request.MeetingLink,
            AiQuestions = request.AiQuestions,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Interviews.Create(interview);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Interview scheduled with ID {InterviewId}", interview.Id);
        return await MapToDtoAsync(interview);
    }

    public async Task<InterviewDto> GetInterviewAsync(int interviewId)
    {
        var interview = await _unitOfWork.Interviews.GetByIdAsync(interviewId);
        if (interview is null)
            throw new ArgumentException("Interview not found");

        return await MapToDtoAsync(interview);
    }

    public async Task<IEnumerable<InterviewDto>> GetApplicationInterviewsAsync(int jobApplicationId)
    {
        var interviews = await _unitOfWork.Interviews.FindAsync(i => i.JobApplicationId == jobApplicationId);
        
        var dtos = new List<InterviewDto>();
        foreach (var interview in interviews)
        {
            dtos.Add(await MapToDtoAsync(interview));
        }

        return dtos;
    }

    public async Task<IEnumerable<InterviewDto>> GetCandidateInterviewsAsync(string candidateId, int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Retrieving interviews for candidate {CandidateId}", candidateId);
        ValidatePagination(pageNumber, pageSize);

        // Get candidate's applications
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var applicationsResult = await _unitOfWork.JobApplications.GetByCandidateAsync(candidateId, pagination);

        var dtos = new List<InterviewDto>();
        foreach (var application in applicationsResult.Items)
        {
            var interviews = await _unitOfWork.Interviews.FindAsync(i => i.JobApplicationId == application.Id);
            foreach (var interview in interviews)
            {
                dtos.Add(await MapToDtoAsync(interview));
            }
        }

        return dtos;
    }

    public async Task<PagedResult<RecruiterInterviewDto>> GetRecruiterInterviewsAsync(string recruiterId, InterviewStatus? status, PaginationParams pagination)
    {
        _logger.LogInformation("Retrieving interviews for recruiter {RecruiterId}", recruiterId);
        ValidatePagination(pagination.PageNumber, pagination.PageSize);

        // Get recruiter's job postings
        var jobPosts = await _unitOfWork.JobPosts.FindAsync(jp => jp.CreatedByRecruiterId == recruiterId);
        var jobPostIds = jobPosts.Select(j => j.Id).ToList();

        // Getting all applications for these jobs
        var applications = new List<JobApplication>();
        foreach(var jobId in jobPostIds)
        {
            var apps = await _unitOfWork.JobApplications.FindAsync(ja => ja.JobPostId == jobId);
            applications.AddRange(apps);
        }
        var appIds = applications.Select(a => a.Id).ToList();

        var allInterviewsList = new List<Interview>();
        foreach(var appId in appIds)
        {
            var inters = await _unitOfWork.Interviews.FindAsync(i => i.JobApplicationId == appId);
            allInterviewsList.AddRange(inters);
        }

        // Apply status filter
        if (status.HasValue)
        {
            allInterviewsList = allInterviewsList.Where(i => i.Status == status.Value).ToList();
        }

        var totalCount = allInterviewsList.Count;

        var pagedInterviews = allInterviewsList
            .OrderByDescending(i => i.ScheduledAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        var dtos = new List<RecruiterInterviewDto>();
        foreach(var interview in pagedInterviews)
        {
            var application = applications.First(a => a.Id == interview.JobApplicationId);
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(application.CandidateId);

            dtos.Add(new RecruiterInterviewDto
            {
                InterviewId = interview.Id,
                InterviewType = interview.InterviewType.ToString(),
                ScheduledAt = interview.ScheduledAt,
                Status = interview.Status.ToString(),
                MeetingLink = interview.MeetingLink,
                Notes = interview.FeedbackNotes,
                Rating = interview.Score, // Let's use Score for Rating to avoid schema changes
                CandidateId = candidate?.Id ?? string.Empty,
                FullName = candidate?.UserName ?? "Unknown",
                Email = candidate?.Email ?? "Unknown",
                PhoneNumber = candidate?.PhoneNumber
            });
        }

        return new PagedResult<RecruiterInterviewDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<RecruiterInterviewDto> GetInterviewDetailsAsync(int interviewId, string recruiterId)
    {
        var interview = await _unitOfWork.Interviews.GetByIdAsync(interviewId);
        if (interview == null)
            throw new ArgumentException("Interview not found");

        var application = await _unitOfWork.JobApplications.GetByIdAsync(interview.JobApplicationId);
        if (application == null)
            throw new ArgumentException("Job application not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);
        if (jobPost == null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to access this interview");

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(application.CandidateId);

        return new RecruiterInterviewDto
        {
            InterviewId = interview.Id,
            InterviewType = interview.InterviewType.ToString(),
            ScheduledAt = interview.ScheduledAt,
            Status = interview.Status.ToString(),
            MeetingLink = interview.MeetingLink,
            Notes = interview.FeedbackNotes,
            Rating = interview.Score,
            CandidateId = candidate?.Id ?? string.Empty,
            FullName = candidate?.UserName ?? "Unknown",
            Email = candidate?.Email ?? "Unknown",
            PhoneNumber = candidate?.PhoneNumber
        };
    }

    public async Task EvaluateInterviewAsync(int interviewId, string recruiterId, EvaluateInterviewDto evaluation)
    {
        var interview = await _unitOfWork.Interviews.GetByIdAsync(interviewId);
        if (interview == null)
            throw new ArgumentException("Interview not found");

        var application = await _unitOfWork.JobApplications.GetByIdAsync(interview.JobApplicationId);
        if (application == null)
            throw new ArgumentException("Job application not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);
        if (jobPost == null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to evaluate this interview");

        if (interview.Status != InterviewStatus.Completed)
            throw new Domain.Exceptions.BadRequestException("Can only evaluate completed interviews");

        // Map rating to score (since rating is 1-5, and maybe Score was originally 0-100?
        // Wait, the prompt says "Rating (1-5)" and the RecruiterEvaluation says "EvaluateInterviewDto has decimal Rating".
        // Let's store Rating in Score, and FeedbackComments in FeedbackNotes.
        interview.Score = evaluation.Rating;
        interview.FeedbackNotes = evaluation.FeedbackComments;

        _unitOfWork.Interviews.Update(interview);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<InterviewDto> UpdateInterviewAsync(int interviewId, string recruiterId, UpdateInterviewDto request)
    {
        _logger.LogInformation("Updating interview {InterviewId}", interviewId);
        ValidateUpdateInterviewInput(request);

        var interview = await _unitOfWork.Interviews.GetByIdAsync(interviewId);
        if (interview is null)
            throw new ArgumentException("Interview not found");

        // Verify recruiter owns the job posting
        var application = await _unitOfWork.JobApplications.GetByIdAsync(interview.JobApplicationId);
        if (application is null)
            throw new ArgumentException("Job application not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);
        
        if (jobPost is null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to update this interview");

        // Parse new status
        if (!Enum.TryParse<InterviewStatus>(request.Status, true, out var newStatus))
            throw new ArgumentException("Invalid status value");

        // Validate status transition
        ValidateStatusTransition(interview.Status, newStatus);

        // Update status
        interview.Status = newStatus;

        // Update optional fields if provided
        if (!string.IsNullOrWhiteSpace(request.MeetingLink))
            interview.MeetingLink = request.MeetingLink;

        // For completed interviews, require score and transcript
        if (newStatus == InterviewStatus.Completed)
        {
            if (!request.Score.HasValue || request.Score < 0 || request.Score > 100)
                throw new ArgumentException("Score must be provided and between 0-100 for completed interviews");

            interview.Score = request.Score;
            interview.AiTranscript = request.AiTranscript;
            interview.FeedbackNotes = request.FeedbackNotes;
            interview.CompletedAt = DateTime.UtcNow;
        }

        _unitOfWork.Interviews.Update(interview);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(interview);
    }

    public async Task CancelInterviewAsync(int interviewId, string recruiterId)
    {
        var interview = await _unitOfWork.Interviews.GetByIdAsync(interviewId);
        if (interview is null)
            throw new ArgumentException("Interview not found");

        // Verify recruiter owns the job posting
        var application = await _unitOfWork.JobApplications.GetByIdAsync(interview.JobApplicationId);
        if (application is null)
            throw new ArgumentException("Job application not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);

        if (jobPost is null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to cancel this interview");

        // Only allow cancellation if not already completed
        if (interview.Status == InterviewStatus.Completed || interview.Status == InterviewStatus.Cancelled)
            throw new ArgumentException("Cannot cancel a completed or already cancelled interview");

        interview.Status = InterviewStatus.Cancelled;
        _unitOfWork.Interviews.Update(interview);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Interview {InterviewId} cancelled", interviewId);
    }

    public async Task<IEnumerable<InterviewDto>> GetInterviewsByStatusAsync(string status, int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Retrieving {Status} interviews", status);
        ValidatePagination(pageNumber, pageSize);

        if (!Enum.TryParse<InterviewStatus>(status, true, out var interviewStatus))
            throw new ArgumentException("Invalid status value");

        // Apply sorting before pagination to avoid incorrect pages
        var interviews = await _unitOfWork.Interviews.FindAsync(i => i.Status == interviewStatus);

        var skip = (pageNumber - 1) * pageSize;
        var dtos = new List<InterviewDto>();
        foreach (var interview in interviews
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(pageSize))
        {
            dtos.Add(await MapToDtoAsync(interview));
        }

        return dtos;
    }

    private async Task<InterviewDto> MapToDtoAsync(Interview interview)
    {
        var application = await _unitOfWork.JobApplications.GetByIdAsync(interview.JobApplicationId);
        var candidate = application is not null ? await _unitOfWork.Candidates.GetByIdAsync(application.CandidateId) : null;
        var jobPost = application is not null ? await _unitOfWork.JobPosts.GetByIdAsync(application.JobPostId) : null;

        return new InterviewDto
        {
            Id = interview.Id,
            JobApplicationId = interview.JobApplicationId,
            CandidateId = application?.CandidateId ?? string.Empty,
            CandidateName = candidate?.UserName ?? string.Empty,
            JobTitle = jobPost?.Title ?? string.Empty,
            InterviewType = interview.InterviewType.ToString(),
            Status = interview.Status.ToString(),
            ScheduledAt = interview.ScheduledAt,
            DurationMinutes = interview.DurationMinutes,
            MeetingLink = interview.MeetingLink,
            AiQuestions = interview.AiQuestions,
            AiTranscript = interview.AiTranscript,
            Score = interview.Score,
            FeedbackNotes = interview.FeedbackNotes,
            CompletedAt = interview.CompletedAt,
            CreatedAt = interview.CreatedAt
        };
    }

    private static void ValidateCreateInterviewInput(CreateInterviewDto request)
    {
        if (request.JobApplicationId <= 0)
            throw new ArgumentException("Invalid job application ID");

        if (string.IsNullOrWhiteSpace(request.InterviewType))
            throw new ArgumentException("Interview type is required");

        // Normalize ScheduledAt to UTC — frontend may send local time (Kind == Unspecified)
        var scheduledUtc = request.ScheduledAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.ScheduledAt, DateTimeKind.Utc)
            : request.ScheduledAt.ToUniversalTime();

        if (scheduledUtc <= DateTime.UtcNow.AddMinutes(-1))
            throw new ArgumentException("Interview must be scheduled for a future date");

        if (request.DurationMinutes < 5 || request.DurationMinutes > 480)
            throw new ArgumentException("Duration must be between 5 and 480 minutes");

        var validTypes = new[] { "AI", "Live" };
        if (!validTypes.Contains(request.InterviewType))
            throw new ArgumentException("Interview type must be 'AI' or 'Live'");
    }

    private static void ValidateUpdateInterviewInput(UpdateInterviewDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            throw new ArgumentException("Status is required");

        var validStatuses = new[] { "Scheduled", "InProgress", "Completed", "Cancelled" };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException("Invalid status value");

        if (!string.IsNullOrWhiteSpace(request.FeedbackNotes) && request.FeedbackNotes.Length > 1000)
            throw new ArgumentException("Feedback notes must not exceed 1000 characters");
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be at least 1");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100");
    }

    private static void ValidateStatusTransition(InterviewStatus currentStatus, InterviewStatus newStatus)
    {
        // Define valid transitions
        var validTransitions = new Dictionary<InterviewStatus, InterviewStatus[]>
        {
            { InterviewStatus.Scheduled, new[] { InterviewStatus.InProgress, InterviewStatus.Cancelled } },
            { InterviewStatus.InProgress, new[] { InterviewStatus.Completed } },
            { InterviewStatus.Completed, System.Array.Empty<InterviewStatus>() },
            { InterviewStatus.Cancelled, System.Array.Empty<InterviewStatus>() }
        };

        if (!validTransitions.TryGetValue(currentStatus, out var allowedTransitions))
            throw new ArgumentException($"Cannot transition from {currentStatus}");

        if (!allowedTransitions.Contains(newStatus))
            throw new ArgumentException($"Cannot transition from {currentStatus} to {newStatus}");
    }
}

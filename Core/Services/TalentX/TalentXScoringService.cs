using System.Text.Json;
using Domain.Contracts;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions;
using Services.Abstractions.TalentX;
using Services.Abstractions.TalentX.Models;
using Shared.Configuration;

namespace Services.TalentX;

/// <summary>
/// Webhook-first orchestration for TalentX resume scoring with polling fallback.
/// </summary>
public class TalentXScoringService : ITalentXScoringService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITalentXResumeScorerClient _talentXClient;
    private readonly IJobDescriptionBuilder _jobDescriptionBuilder;
    private readonly IFileStorageService _fileStorage;
    private readonly TalentXSettings _settings;
    private readonly ILogger<TalentXScoringService> _logger;

    public TalentXScoringService(
        IUnitOfWork unitOfWork,
        ITalentXResumeScorerClient talentXClient,
        IJobDescriptionBuilder jobDescriptionBuilder,
        IFileStorageService fileStorage,
        IOptions<TalentXSettings> options,
        ILogger<TalentXScoringService> logger)
    {
        _unitOfWork = unitOfWork;
        _talentXClient = talentXClient;
        _jobDescriptionBuilder = jobDescriptionBuilder;
        _fileStorage = fileStorage;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task InitiateScoringForApplicationAsync(
        int jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("TalentX integration disabled; skipping scoring for application {Id}", jobApplicationId);
            return;
        }

        var application = await _unitOfWork.JobApplications.GetByIdAsync(jobApplicationId);
        if (application is null)
        {
            _logger.LogWarning("Job application {Id} not found for TalentX scoring", jobApplicationId);
            return;
        }

        if (application.AiScoringStatus == AiScoringStatus.Processing
            && application.AiScoringRequestedAt > DateTime.UtcNow.AddMinutes(-5))
        {
            _logger.LogInformation(
                "Application {Id} already in TalentX processing; skipping duplicate initiation",
                jobApplicationId);
            return;
        }

        var resume = await _unitOfWork.Resumes.GetByIdAsync(application.ResumeId);
        if (resume is null)
            throw new NotFoundException("Resume not found for scoring.");

        var jobPost = await _unitOfWork.JobPosts.GetByIdWithSkillsAsync(application.JobPostId, cancellationToken);
        if (jobPost is null)
            throw new NotFoundException("Job post not found for scoring.");

        if (!_fileStorage.FileExists(resume.StoredFilePath))
            throw new BadRequestException("Resume file is missing from storage.");

        var jobDescriptionJson = _jobDescriptionBuilder.BuildJson(jobPost);
        var idempotencyKey = TalentXIdMapper.BuildIdempotencyKey(application.CandidateId, application.JobPostId);

        await using var cvStream = await _fileStorage.GetFileStreamAsync(resume.StoredFilePath);

        var request = new TalentXScoreRequest
        {
            CandidateId = application.CandidateId,
            JobId = application.JobPostId,
            CvFileStream = cvStream,
            CvFileName = resume.OriginalFileName,
            JobDescriptionJson = jobDescriptionJson
        };

        await _talentXClient.InitiateScoringAsync(request, idempotencyKey, cancellationToken);

        application.AiScoringStatus = AiScoringStatus.Processing;
        application.AiScoringRequestedAt = DateTime.UtcNow;
        application.AiScoringCompletedAt = null;
        application.AiScoringErrorMessage = null;
        application.MatchScore = null;

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "TalentX scoring initiated for application {ApplicationId} (candidate {CandidateId}, job {JobId})",
            jobApplicationId,
            application.CandidateId,
            application.JobPostId);
    }

    public async Task<bool> ProcessWebhookAsync(
        TalentXWebhookPayload payload,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new BadRequestException("X-Idempotency-Key header is required.");

        if (await _unitOfWork.ProcessedIdempotencyKeys.ExistsAsync(idempotencyKey, cancellationToken))
        {
            _logger.LogInformation(
                "Duplicate TalentX webhook ignored for idempotency key {Key}",
                idempotencyKey);
            return false;
        }

        var application = await _unitOfWork.JobApplications.GetByTalentXCorrelationAsync(
            payload.CandidateId,
            payload.JobId,
            cancellationToken);

        if (application is null)
        {
            _logger.LogWarning(
                "No job application found for TalentX webhook candidate {CandidateId}, job {JobId}",
                payload.CandidateId,
                payload.JobId);
            throw new NotFoundException("Job application not found for webhook correlation.");
        }

        ApplyPayloadToApplication(application, payload);

        application.LastProcessedIdempotencyKey = idempotencyKey;

        await _unitOfWork.ProcessedIdempotencyKeys.AddAsync(
            new ProcessedIdempotencyKey
            {
                IdempotencyKey = idempotencyKey,
                JobApplicationId = application.Id,
                Source = "TalentXWebhook",
                ProcessedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "TalentX webhook processed for application {ApplicationId}, status {Status}",
            application.Id,
            payload.Status);

        return true;
    }

    public async Task<bool> PollAndApplyResultAsync(
        int jobApplicationId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
            return false;

        var application = await _unitOfWork.JobApplications.GetByIdAsync(jobApplicationId);
        if (application is null
            || application.AiScoringStatus != AiScoringStatus.Processing)
            return false;

        var result = await _talentXClient.GetResultsAsync(
            application.CandidateId,
            application.JobPostId,
            cancellationToken);

        if (result is null || !IsTerminalStatus(result.Status))
            return false;

        var pollKey = result.IdempotencyKey
            ?? $"talentx-poll-{application.CandidateId}-{application.JobPostId}-{DateTime.UtcNow:O}";

        if (await _unitOfWork.ProcessedIdempotencyKeys.ExistsAsync(pollKey, cancellationToken))
            return false;

        ApplyResultToApplication(application, result);

        application.LastProcessedIdempotencyKey = pollKey;

        await _unitOfWork.ProcessedIdempotencyKeys.AddAsync(
            new ProcessedIdempotencyKey
            {
                IdempotencyKey = pollKey,
                JobApplicationId = application.Id,
                Source = "TalentXPolling",
                ProcessedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        _unitOfWork.JobApplications.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "TalentX polling applied result for application {ApplicationId}, status {Status}",
            application.Id,
            result.Status);

        return true;
    }

    private void ApplyPayloadToApplication(JobApplication application, TalentXWebhookPayload payload)
    {
        application.RawResumeDataJson = SerializeJsonElement(payload.RawResumeData);
        application.MatchingDetailsJson = SerializeJsonElement(payload.MatchingDetails);
        application.FitStatus = payload.FitStatus;
        application.AiScoringCompletedAt = DateTime.UtcNow;

        if (string.Equals(payload.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            application.AiScoringStatus = AiScoringStatus.Completed;
            application.MatchScore = payload.FinalScore.HasValue
                ? Math.Round(payload.FinalScore.Value, 2)
                : 0m;
            application.AiScoringErrorMessage = null;
            application.MatchReport = BuildMatchReport(payload.FitStatus, payload.FinalScore);

            // Automatically update application status based on FitStatus
            if (string.Equals(payload.FitStatus, "not_fit", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(payload.FitStatus, "not fit", StringComparison.OrdinalIgnoreCase))
            {
                application.Status = ApplicationStatus.Rejected;
            }
            else if (string.Equals(payload.FitStatus, "fit", StringComparison.OrdinalIgnoreCase) || 
                     string.Equals(payload.FitStatus, "partial_fit", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(payload.FitStatus, "partial fit", StringComparison.OrdinalIgnoreCase))
            {
                application.Status = ApplicationStatus.UnderReview;
            }
        }
        else if (string.Equals(payload.Status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            application.AiScoringStatus = AiScoringStatus.Failed;
            application.MatchScore = 0m;
            application.AiScoringErrorMessage = payload.ErrorMessage;
            application.MatchReport = payload.ErrorMessage;
        }
        else
        {
            application.AiScoringStatus = AiScoringStatus.Processing;
        }
    }

    private void ApplyResultToApplication(JobApplication application, TalentXScoreResultResponse result)
    {
        application.RawResumeDataJson = SerializeJsonElement(result.RawResumeData);
        application.MatchingDetailsJson = SerializeJsonElement(result.MatchingDetails);
        application.FitStatus = result.FitStatus;
        application.AiScoringCompletedAt = DateTime.UtcNow;

        if (string.Equals(result.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            application.AiScoringStatus = AiScoringStatus.Completed;
            application.MatchScore = result.FinalScore.HasValue
                ? Math.Round(result.FinalScore.Value, 2)
                : 0m;
            application.AiScoringErrorMessage = null;
            application.MatchReport = BuildMatchReport(result.FitStatus, result.FinalScore);

            // Automatically update application status based on FitStatus
            if (string.Equals(result.FitStatus, "not_fit", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(result.FitStatus, "not fit", StringComparison.OrdinalIgnoreCase))
            {
                application.Status = ApplicationStatus.Rejected;
            }
            else if (string.Equals(result.FitStatus, "fit", StringComparison.OrdinalIgnoreCase) || 
                     string.Equals(result.FitStatus, "partial_fit", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(result.FitStatus, "partial fit", StringComparison.OrdinalIgnoreCase))
            {
                application.Status = ApplicationStatus.UnderReview;
            }
        }
        else if (string.Equals(result.Status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            application.AiScoringStatus = AiScoringStatus.Failed;
            application.MatchScore = 0m;
            application.AiScoringErrorMessage = result.ErrorMessage;
            application.MatchReport = result.ErrorMessage;
        }
    }

    private static bool IsTerminalStatus(string? status) =>
        string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase);

    private static string? SerializeJsonElement(JsonElement? element)
    {
        if (element is null || element.Value.ValueKind == JsonValueKind.Null)
            return null;

        return element.Value.GetRawText();
    }

    private static string? BuildMatchReport(string? fitStatus, decimal? score) =>
        fitStatus is null && score is null
            ? null
            : $"fit_status={fitStatus ?? "n/a"}; score={score?.ToString("F1") ?? "n/a"}";
}

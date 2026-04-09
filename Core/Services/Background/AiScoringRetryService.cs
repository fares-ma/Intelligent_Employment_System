using Domain.Contracts;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services.Background;

public class AiScoringRetryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AiScoringRetryService> _logger;

    public AiScoringRetryService(IServiceProvider serviceProvider, ILogger<AiScoringRetryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AiScoringRetryService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var aiClient = scope.ServiceProvider.GetRequiredService<IAiServiceClient>();

                var applicationsWithoutScore = await unitOfWork.JobApplications.FindAsync(
                    ja => ja.MatchScore == null && ja.Status != ApplicationStatus.Pending);

                foreach (var application in applicationsWithoutScore)
                {
                    try
                    {
                        var resume = await unitOfWork.Resumes.GetByIdAsync(application.ResumeId);
                        var jobPost = await unitOfWork.JobPosts.GetByIdAsync(application.JobPostId);

                        if (resume is not null && jobPost is not null)
                        {
                            var (score, _) = await aiClient.ScoreResumeAsync(resume.ExperienceSummary ?? "", jobPost.Description);
                            application.MatchScore = score;
                            unitOfWork.JobApplications.Update(application);
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _logger.LogWarning(ex, "Failed to score application {ApplicationId}", application.Id);
                        }
                        catch
                        {
                            // Ignore logger disposal failures during shutdown.
                        }
                    }
                }

                if (applicationsWithoutScore.Any())
                {
                    await unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Retried AI scoring for {Count} applications", applicationsWithoutScore.Count());
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(ex, "Error in AiScoringRetryService");
                }
                catch
                {
                    // Ignore logger disposal failures during shutdown.
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}

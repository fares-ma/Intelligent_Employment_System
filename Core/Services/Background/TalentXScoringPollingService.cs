using Domain.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions.TalentX;
using Shared.Configuration;

namespace Services.Background;

/// <summary>
/// Fallback only: polls TalentX for applications stuck in Processing when webhooks are delayed or lost.
/// Primary path remains webhook-based (see TalentXWebhookController).
/// </summary>
public class TalentXScoringPollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TalentXScoringPollingService> _logger;

    public TalentXScoringPollingService(
        IServiceProvider serviceProvider,
        ILogger<TalentXScoringPollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TalentXScoringPollingService started (webhook fallback only)");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunPollingCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TalentXScoringPollingService cycle");
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

    private async Task RunPollingCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var settings = scope.ServiceProvider.GetRequiredService<IOptions<TalentXSettings>>().Value;

        if (!settings.Enabled)
            return;

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var scoringService = scope.ServiceProvider.GetRequiredService<ITalentXScoringService>();

        var stuck = await unitOfWork.JobApplications.GetStuckInProcessingAsync(
            TimeSpan.FromMinutes(settings.PollingFallbackAfterMinutes),
            settings.PollingBatchSize,
            cancellationToken);

        if (stuck.Count == 0)
            return;

        _logger.LogInformation("Polling TalentX for {Count} stuck applications", stuck.Count);

        var recovered = 0;
        foreach (var application in stuck)
        {
            try
            {
                if (await scoringService.PollAndApplyResultAsync(application.Id, cancellationToken))
                    recovered++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "TalentX polling failed for application {ApplicationId}",
                    application.Id);
            }
        }

        if (recovered > 0)
            _logger.LogInformation("TalentX polling recovered {Count} application scores", recovered);
    }
}

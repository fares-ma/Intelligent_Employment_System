using Domain.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Services.Background;

public class JobExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobExpiryBackgroundService> _logger;

    public JobExpiryBackgroundService(IServiceProvider serviceProvider, ILogger<JobExpiryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobExpiryBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expiredJobs = await unitOfWork.JobPosts.FindAsync(
                    jp => jp.IsActive && jp.ExpiryDate.HasValue && jp.ExpiryDate.Value < DateTime.UtcNow);

                foreach (var job in expiredJobs)
                {
                    job.IsActive = false;
                    unitOfWork.JobPosts.Update(job);
                }

                if (expiredJobs.Any())
                {
                    await unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Deactivated {Count} expired job postings", expiredJobs.Count());
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
                    _logger.LogError(ex, "Error in JobExpiryBackgroundService");
                }
                catch
                {
                    // Ignore logger disposal failures during shutdown.
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}

using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services;

/// <summary>
/// Dummy Email Service implementation.
/// Logs emails to console for development.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("--- MOCK EMAIL SENT ---");
        _logger.LogInformation("To: {To}", to);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Body: {Body}", body);
        _logger.LogInformation("-----------------------");
        
        return Task.CompletedTask;
    }
}

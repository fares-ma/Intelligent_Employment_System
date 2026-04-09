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
        var maskedTo = to.Contains("@") ? $"{to[0]}***@{to.Split('@')[1]}" : "***";
        
        _logger.LogInformation("--- MOCK EMAIL SENT ---");
        _logger.LogDebug("To: {To}", maskedTo);
        _logger.LogDebug("Subject: {Subject}", subject);
        _logger.LogDebug("Body: {Body}", body);
        _logger.LogInformation("-----------------------");
        
        return Task.CompletedTask;
    }
}

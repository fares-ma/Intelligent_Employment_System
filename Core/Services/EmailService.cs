using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Services.Abstractions;

namespace Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

        if (environment == "Development")
        {
            _logger.LogInformation("[DEV EMAIL] To: {To}, Subject: {Subject}, Body: {Body}", to, subject, body);
        }
        else if (environment == "Staging")
        {
            _logger.LogInformation("[STAGING EMAIL] To: {To}, Subject: {Subject}", to, subject);
        }

        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = "Welcome to Intelligent Employment System";
        var body = $"Hello {firstName},\n\nWelcome! Your account has been created successfully.\n\nBest regards,\nIES Team";
        return SendEmailAsync(email, subject, body);
    }

    public Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        var subject = "Reset Your Password";
        var body = $"Click the link to reset your password: {resetLink}\n\nThis link expires in 24 hours.\n\nBest regards,\nIES Team";
        return SendEmailAsync(email, subject, body);
    }

    public Task SendNotificationEmailAsync(string email, string title, string message)
    {
        var subject = $"IES Notification: {title}";
        var body = $"{message}\n\nLog in to view details.\n\nBest regards,\nIES Team";
        return SendEmailAsync(email, subject, body);
    }
}

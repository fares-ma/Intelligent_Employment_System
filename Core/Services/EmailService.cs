using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Services.Abstractions;
using Services.Abstractions.DTOs.Settings;

namespace Services;

/// <summary>
/// Email Service implementation using MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var maskedTo = to.Contains("@") ? $"{to[0]}***@{to.Split('@')[1]}" : "***";
        
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            
            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            
            // Connect to the SMTP server
            await smtp.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            
            // Authenticate if credentials are provided
            if (!string.IsNullOrEmpty(_emailSettings.SenderEmail) && !string.IsNullOrEmpty(_emailSettings.SenderPassword))
            {
                await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
            }
            
            // Send the email
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {To}", maskedTo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", maskedTo);
            // Don't throw here to avoid disrupting the main flow if email fails, 
            // unless we strictly want to fail the operation.
        }
    }
}

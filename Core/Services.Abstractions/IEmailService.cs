namespace Services.Abstractions;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendPasswordResetEmailAsync(string email, string resetLink);
    Task SendNotificationEmailAsync(string email, string title, string message);
}

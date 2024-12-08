namespace Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailMailgunAsync(string to, string subject, string body);
}
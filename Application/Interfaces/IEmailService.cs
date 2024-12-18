using Core.Entities;

namespace Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(User user, string subject, string body);
    Task ForceSendEmailAsync(User user, string subject, string body);

    Task SendEmailAsync(User user, string subject, string htmlContent, Stream fileContent = null,
        string fileName = null, string contentType = "application/octet-stream");
    Task SendEmailMailgunAsync(User user, string subject, string body);
}
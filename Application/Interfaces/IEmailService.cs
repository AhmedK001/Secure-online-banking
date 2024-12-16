namespace Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);

    Task SendEmailAsync(string toEmail, string subject, string htmlContent, Stream fileContent = null,
        string fileName = null, string contentType = "application/octet-stream");
    Task SendEmailMailgunAsync(string to, string subject, string body);
}
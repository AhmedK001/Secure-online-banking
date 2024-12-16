using System.Text;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly string _mailGunDomain;
    private readonly string _mailGunApiKey;
    private readonly string _mailGunSenderEmail;
    private readonly string _domain;
    private readonly string _apiKey;
    private readonly string _senderEmail;

    private readonly HttpClient _httpClient;

    public EmailService(IConfiguration configuration)
    {
        _mailGunDomain = configuration["Mailgun:Domain"];
        _mailGunApiKey = configuration["Mailgun:ApiKey"];
        _mailGunSenderEmail = configuration["Mailgun:SenderEmail"];
        _apiKey = configuration["SendGrid:ApiKey"];
        _senderEmail = configuration["SendGrid:SenderEmail"];
        _httpClient = new HttpClient();
    }

    public async Task SendEmailMailgunAsync(string to, string subject, string body)
    {
        var url = $"https://api.mailgun.net/v3/{_mailGunDomain}/messages";

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Headers
                =
                {
                    {
                        "Authorization",
                        "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_mailGunApiKey}"))
                    }
                },
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("from", _mailGunSenderEmail),
                new KeyValuePair<string, string>("to", to),
                new KeyValuePair<string, string>("subject", subject),
                new KeyValuePair<string, string>("html", body)
            })
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var client = new SendGridClient(_apiKey);

        var from = new EmailAddress(_senderEmail, "SecureOnlineBanking");

        var to = new EmailAddress(toEmail);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlContent);

        var response = await client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to send email. Status Code: {response.StatusCode}");
        }
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, Stream fileContent = null,
        string fileName = null, string contentType = "application/octet-stream")
    {
        var client = new SendGridClient(_apiKey);

        var from = new EmailAddress(_senderEmail, "SecureOnlineBanking");

        var to = new EmailAddress(toEmail);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlContent);

        // Check if a file attachment is provided
        if (fileContent != null && !string.IsNullOrEmpty(fileName))
        {
            using (var memoryStream = new MemoryStream())
            {
                await fileContent.CopyToAsync(memoryStream);
                var base64File = Convert.ToBase64String(memoryStream.ToArray());

                msg.AddAttachment(fileName, base64File, contentType);
            }
        }

        var response = await client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to send email. Status Code: {response.StatusCode}");
        }
    }
}
using System.Text;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class EmailsService : IEmailService
{
    private readonly string _domain;
    private readonly string _apiKey;
    private readonly string _senderEmail;
    private readonly HttpClient _httpClient;

    public EmailsService(IConfiguration configuration)
    {
        _domain = configuration["Mailgun:Domain"];
        _apiKey = configuration["Mailgun:ApiKey"];
        _senderEmail = configuration["Mailgun:SenderEmail"];
        _httpClient = new HttpClient();
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var url = $"https://api.mailgun.net/v3/{_domain}/messages";

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Headers
                =
                {
                    { "Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_apiKey}")) }
                },
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("from", _senderEmail),
                new KeyValuePair<string, string>("to", to),
                new KeyValuePair<string, string>("subject", subject),
                new KeyValuePair<string, string>("html", body)
            })
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
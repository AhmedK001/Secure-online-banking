namespace Application.DTOs;

public class AuthenticationResponse
{
    public string UserName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpirationTime { get; set; } 
}
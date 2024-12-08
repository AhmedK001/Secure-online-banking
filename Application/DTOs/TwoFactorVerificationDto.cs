namespace Application.DTOs;

public class TwoFactorVerificationDto
{
    public string Code { get; set; }
    public string EmailAddress { get; set; }
}
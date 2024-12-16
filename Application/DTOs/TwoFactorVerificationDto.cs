using System.ComponentModel;

namespace Application.DTOs;

public class TwoFactorVerificationDto
{
    [DefaultValue("")]
    public string Code { get; set; }
    [DefaultValue("@gmail.com")]
    public string EmailAddress { get; set; }
}
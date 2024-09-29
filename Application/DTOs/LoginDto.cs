using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class LoginDto
{
    [MinLength(8)]
    [Required(ErrorMessage = "Email address is required.")]
    public string EmailAddress { get; set; } = string.Empty;

    [MinLength(8)]
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
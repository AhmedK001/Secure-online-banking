using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address. ")]
    public required string Email { get; set; }

    public required string Token { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,}$",
        ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, and one number.")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords does not match.")]
    [DataType(DataType.Password)]
    public string PasswordConfirmation { get; set; }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class EmailDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [DefaultValue("@gmail.com")]
    [EmailAddress(ErrorMessage = "Invalid email address. ")]
    public required string Email { get; set; }
}
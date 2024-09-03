using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RegistrationDTOs;

public class RegisterUserDTO
{
    [Required(ErrorMessage = "National ID cannot be empty.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID number must consist of exactly 10 digits")]
    public string NationalId { get; set; }
    [Required(ErrorMessage = "First name is required..")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required..")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Date of birth is required.")]
    [RegularExpression(@"^(19|20)\d{2}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$", ErrorMessage = "Date of Birth must be in the format yyyy-MM-dd")]
    public DateTime DateOfBirth { get; set; }
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^0\d{9}$",ErrorMessage = "Phone number must be consist of 10 digits, Starts with 0.")]
    public string PhoneNumber { get; set; }
    
    
    public override string ToString()
    {
        return $"NationalId: {NationalId}, " +
               $"FirstName: {FirstName}, " +
               $"LastName: {LastName}, "+
               $"DateOfBirth: {DateOfBirth.ToString("yyyy-MM-dd")}, " +
               $"Email: {Email}, " +
               $"PhoneNumber: {PhoneNumber}"
               ;
    }
}
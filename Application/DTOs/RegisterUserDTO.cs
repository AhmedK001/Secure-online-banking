using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Application.DTOs.RegistrationDTOs;

public class RegisterUserDto
{
    [Required(ErrorMessage = "National ID cannot be empty.")]
    [RegularExpression(@"^11\d{8}$",
        ErrorMessage
            = "National ID number must consist of exactly 10 digits. Starts with number 11.")]
    public required string NationalId { get; set; }

    [Required(ErrorMessage = "First name is required..")]
    [RegularExpression("^[A-Z][a-zA-Z]{1,19}$",
        ErrorMessage
            = "First name must start with capital letter. And has a least Two letters.")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required..")]
    [RegularExpression("^[A-Z][a-zA-Z]{1,19}$",
        ErrorMessage
            = "Last name must start with capital letter. And has a least Two letters.")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Date of birth is required.")]
    //[RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date of Birth must be in the format yyyy-MM-dd")]
    [DataType(DataType.Date, ErrorMessage = "Invalid birth date format. Please use yyyy-MM-dd.")]
    public string DateOfBirth { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address. ")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^05\d{8}$",
        ErrorMessage
            = "Phone number must be consist of 10 digits, Starts with 05.")]
    public required string PhoneNumber { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,}$", 
        ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, and one number.")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords does not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    
    public override string ToString()
    {
        return $"NationalId: {NationalId}, " + $"FirstName: {FirstName}, " +
               $"LastName: {LastName}, " +
               $"DateOfBirth: {DateOfBirth}, " +
               $"Email: {Email}, " + $"PhoneNumber: {PhoneNumber}";
    }
}
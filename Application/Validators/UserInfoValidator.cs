using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Application.DTOs.Helpers;
using Core.Entities;

namespace Application.Validators;

public class UserInfoValidator
{
    // Method to validate all user information
    public static ValidationResult IsUserInfoAccepted(User user)
    {
        // Validate National ID
        var nationalIdValidation = IsNationalId(user.NationalId);
        if (nationalIdValidation != ValidationResult.Success)
        {
            return nationalIdValidation;
        }

        // Validate First Name
        var firstNameValidation = IsName(user.FirstName);
        if (firstNameValidation != ValidationResult.Success)
        {
            return firstNameValidation;
        }

        // Validate Last Name
        var lastNameValidation = IsName(user.LastName);
        if (lastNameValidation != ValidationResult.Success)
        {
            return lastNameValidation;
        }

        // Validate Date of Birth
        var birthDateValidation = IsAcceptedBirthDate(user.DateOfBirth.ToString());
        if (birthDateValidation != ValidationResult.Success)
        {
            return birthDateValidation;
        }

        // Validate Email
        var emailValidation = IsEmail(user.Email);
        if (emailValidation != ValidationResult.Success)
        {
            return emailValidation;
        }

        // Validate Phone Number
        var phoneNumberValidation = IsPhoneNumber(user.PhoneNumber);
        if (phoneNumberValidation != ValidationResult.Success)
        {
            return phoneNumberValidation;
        }

        return ValidationResult.Success;
    }

    public static ValidationResult IsNationalId(int? nationalId)
    {
        if (nationalId is null)
        {
            return new ValidationResult("National ID cannot be empty.");
        }

        string nationalIdAsString = nationalId.ToString();

        string pattern = @"^[1-9]\d{9}$";
        Regex regex = new Regex(pattern);
        char firstNationalIdDigit = nationalIdAsString[0];


        if (firstNationalIdDigit == 0)
        {
            return new ValidationResult("National ID number cannot start with 0.");
        }

        if (nationalIdAsString.Length != 10)
        {
            return new ValidationResult("National ID number must be 10 digits.");
        }

        // Make sure of all conditions using regex.
        if (!regex.IsMatch(nationalIdAsString))
        {
            return new ValidationResult("Invalid National ID number.");
        }

        return ValidationResult.Success;
    }

    public static ValidationResult IsName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return new ValidationResult("Name is required..");

        string pattern = @"^[A-Z][a-zA-Z]{0,19}$";
        Regex regex = new Regex(pattern);

        if (!regex.IsMatch(name))
        {
            return new ValidationResult("Invalid name.");
        }

        return ValidationResult.Success;
    }

    public static ValidationResult IsEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ValidationResult("Email address is required.");
        }

        // Improved email regex pattern
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        Regex regex = new Regex(pattern);

        if (!regex.IsMatch(email))
        {
            return new ValidationResult("Invalid email address.");
        }

        return ValidationResult.Success;
    }


    public static ValidationResult IsPhoneNumber(string phoneNumber)
    {
        throw new NotImplementedException(); // not implemented yet
        return ValidationResult.Success;
    }

    public static ValidationResult IsAcceptedBirthDate(string birthDateString)
    {
        if (string.IsNullOrWhiteSpace(birthDateString))
        {
            return new ValidationResult("Birth date is required.");
        }

        // Check if the format is correct
        if (!DateTime.TryParseExact(birthDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out DateTime birthDate))
        {
            return new ValidationResult("Invalid birth date format. Please use yyyy-MM-dd.");
        }

        // Calculate age
        var currentDate = DateTime.Now;
        var age = currentDate.Year - birthDate.Year;

        // Adjust age if the birth date hasn't occurred yet this year
        if (birthDate > currentDate.AddYears(-age))
        {
            age--;
        }

        // Validate age
        if (age < 16 || age > 120)
        {
            return new ValidationResult("The minimum required age is 16.");
        }

        // Validation passed
        return ValidationResult.Success;
    }
}
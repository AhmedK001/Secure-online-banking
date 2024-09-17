using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
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
        var birthDateValidation = IsAcceptedBirthDate(user.DateOfBirth);
        if (birthDateValidation != ValidationResult.Success)
        {
            return birthDateValidation;
        }

        // Validate Email
        var emailValidation = IsEmail(user.UserContactInfo?.Email);
        if (emailValidation != ValidationResult.Success)
        {
            return emailValidation;
        }

        // Validate Phone Number
        var phoneNumberValidation = IsPhoneNumber(user.UserContactInfo?.PhoneNumber);
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
            return new ValidationResult(
                "National ID number cannot start with 0.");
        }

        if (nationalIdAsString.Length != 10)
        {
            return new ValidationResult(
                "National ID number must be 10 digits.");
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

        // if (string.IsNullOrWhiteSpace(phoneNumber))
        //     return new ValidationResult("Phone number is required.");
        //
        // if (!phoneNumber.StartsWith("0"))
        // {
        //     return new ValidationResult("Phone number must start with 0.");
        // }
        //
        // if (phoneNumber.Length != 10)
        // {
        //     return new ValidationResult(
        //         "Phone number must be exactly 10 digits.");
        // }
        //
        // string pattern = @"^0\d{9}$";
        // Regex regex = new Regex(pattern);
        //
        // if (!regex.IsMatch(phoneNumber))
        // {
        //     return new ValidationResult("Invalid phone number.");
        // }
        
        return ValidationResult.Success;
    }

    public static ValidationResult IsAcceptedBirthDate(object birthDate)
    {
        throw new NotImplementedException(); // not implemented yet

        // if (string.IsNullOrWhiteSpace(birthDate.ToString()))
        //     return new ValidationResult("Birth date is required");
        //
        // if (birthDate is not string dateString)
        //     return new ValidationResult("Invalid birth date");
        //
        // string pattern = @"^\d{4}-\d{2}-\d{2}$";
        // Regex regex = new Regex(pattern);
        //
        // if (!regex.IsMatch(dateString))
        // {
        //     return new ValidationResult("Date must be in YYYY-MM-DD format.");
        // }
        //
        // // Try to parse the date
        // if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateOfBirth))
        // {
        //     return new ValidationResult("Date must be in YYYY-MM-DD format.");
        // }
        //
        // if (dateOfBirth > DateTime.Now.AddYears(-16))
        // {
        //     return new ValidationResult("You must be at least 16 years old.");
        // }

        return ValidationResult.Success;
    }

}
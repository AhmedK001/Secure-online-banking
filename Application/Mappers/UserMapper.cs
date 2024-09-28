using System.Globalization;
using Application.DTOs.RegistrationDTOs;
using Core.Entities;

namespace Application.Mappers;

public class UserMapper
{
    public static User ConvertToUserObject(RegisterUserDto userDto)
    {
        // Convert userDto to user object
        User user = new User
        {
            NationalId = int.Parse(userDto.NationalId), // convert National ID as string to int
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            DateOfBirth = DateTime.ParseExact(userDto.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture), // convert date of birth to DateTime format
            Email = userDto.Email, 
            PhoneNumber = userDto.PhoneNumber
        };

        return user;
    }
}
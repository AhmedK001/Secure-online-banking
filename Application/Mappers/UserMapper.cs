using Application.DTOs.RegistrationDTOs;
using Core.Entities;

namespace Application.Mappers;

public class UserMapper
{
    public static User ConvertToUser(RegisterUserDTO userDto)
    {
        // Convert userDto to user object
        User user = new User
        {
            NationalId = int.Parse(userDto.NationalId), // convert National ID as string to int
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            DateOfBirth = userDto.DateOfBirth,
            UserContactInfo = new UserContactInfo
            {
                Email = userDto.Email, PhoneNumber = userDto.PhoneNumber
            }
        };

        return user;
    }
}
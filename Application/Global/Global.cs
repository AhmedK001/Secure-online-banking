using System.Globalization;
using Application.DTOs;
using Core.Entities;
using Core.Enums;

namespace Application.Mappers;

public class Global
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

    public static string FormatCurrency(EnumCurrency currency, decimal balance)
    {
        string currencySymbol = currency switch
        {
            EnumCurrency.SAR => "ريال",
            EnumCurrency.EGP => "E£",
            EnumCurrency.AED => "د.إ",
            EnumCurrency.USD => "$",
            EnumCurrency.EUR => "€",
            EnumCurrency.TRY => "₺",
            _ => currency.ToString(),
        };

        return $"{currencySymbol} {balance:F2}";
    }

    public static string FormatCurrency(EnumCurrency currency)
    {
        string currencySymbol = currency switch
        {
            EnumCurrency.SAR => "ريال",
            EnumCurrency.EGP => "E£",
            EnumCurrency.AED => "د.إ",
            EnumCurrency.USD => "$",
            EnumCurrency.EUR => "€",
            EnumCurrency.TRY => "₺",
            _ => currency.ToString(),
        };

        return $"{currencySymbol}";
    }
}
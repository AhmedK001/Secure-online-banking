using System.Text.RegularExpressions;
using Application.Interfaces;

namespace Application.Validators;

public class Validate : IValidate
{
    public (bool, string) IsContainsNumbers(string value)
    {
        var trimmedValue = value.Trim();

        if (Regex.IsMatch(trimmedValue, "^\\d+$"))
        {
            return (true, "Value contains numbers");
        }

        return (false, "");
    }
}
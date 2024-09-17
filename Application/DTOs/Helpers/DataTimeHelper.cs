namespace Application.DTOs.Helpers;

using System;
using System.Globalization;

public static class DateTimeHelper
{
    public static (DateTime? DateTimeValue, string ErrorMessage) TryParseDateString(string dateString, string format = "yyyy-MM-dd")
    {
        if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
        {
            return (dateTimeValue, null);
        }
        return (null, $"Date must be in the format {format}.");
    }
}

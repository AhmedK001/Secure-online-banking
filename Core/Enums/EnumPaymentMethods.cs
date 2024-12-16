using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum EnumPaymentMethods
{
    pm_card_visa = 0,
    pm_card_mastercard= 1,
}
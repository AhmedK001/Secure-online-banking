using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ExternalModels;

public class CurrencySymbolDto
{
    [Required]
    [RegularExpression("^[A-Za-z]{1,6}$",ErrorMessage = "System accept Stock symbols only.")]
    public string Symbol { get; set; }
}
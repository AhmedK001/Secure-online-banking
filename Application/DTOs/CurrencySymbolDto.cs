using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CurrencySymbolDto
{
    [Required]
    [DefaultValue("EGP")]
    [RegularExpression("^[A-Za-z]{1,6}$",ErrorMessage = "System accept Stock symbols only.")]
    public string Symbol { get; set; }
}
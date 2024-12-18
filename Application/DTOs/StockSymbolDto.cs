using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class StockSymbolDto
{
    [Required]
    [DefaultValue("TSLA")]
    [RegularExpression("^[A-Za-z]{1,6}$",ErrorMessage = "System accept Stock symbols only.")]
    public string Symbol { get; set; }
}
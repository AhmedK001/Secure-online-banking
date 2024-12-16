using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class StockPricesDto
{
    [RegularExpression("^[A-Z]{1,5}(\\.[A-Z]{1,2})?$",ErrorMessage = "You could write up to 5 character.")]
    [DefaultValue("NVDA")]
    public string? Symbol { get; set; }
    [Required(ErrorMessage = "Timestamp is required.")]
    [DefaultValue(30)]
    [RegularExpression($"^(1|5|15|30|60)$",ErrorMessage = "Select one timestamp for stock prices 1, 5, 15, 30, or 60 minutes.")]
    public int Timestamp { get; set; }
}
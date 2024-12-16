using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class BuySellStockDto
{
    [Required]
    [DefaultValue("MSFT")]
    [RegularExpression("^[A-Za-z]{1,6}$",ErrorMessage = "System accept Stock symbols only.")]
    public string Symbol { get; set; }

    [Required(ErrorMessage = "Number of Stock to buy required.")]
    [DefaultValue(0)]
    [RegularExpression("^(?!0$)\\d{1,8}$",ErrorMessage = "Please enter a valid amount.")]
    public int NumberOfStocks { get; set; }
}
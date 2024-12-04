using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class BuyStockDto
{
    [Required]
    [RegularExpression("^[A-Za-z]{1,6}$",ErrorMessage = "System accept Stock symbols only.")]
    public string Symbol { get; set; }

    [Required(ErrorMessage = "Number of Stock to buy required.")]
    [RegularExpression("^(?!0$)\\d{1,8}$",ErrorMessage = "Please enter a valid amount.")]
    public int NumberOfStocks { get; set; }
}
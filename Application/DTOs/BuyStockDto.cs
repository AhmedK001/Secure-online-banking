using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class BuyStockDto
{
    [Required]
    [RegularExpression("^[A-Za-z]{1,6}$",ErrorMessage = "System accept Stock symbols only.")]
    public string Symbol { get; set; }

    [Required(ErrorMessage = "Number of Stock to buy required.")]
    public int NumberOfStocks { get; set; }
}
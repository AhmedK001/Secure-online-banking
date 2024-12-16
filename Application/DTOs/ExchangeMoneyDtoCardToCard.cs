using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ExchangeMoneyDtoCardToCard
{
    [Required]
    [RegularExpression("^\\d{8}$",ErrorMessage = "Please try again with valid Card ID number.")]
    public int BaseCardId { get; set; }
    [Required]
    [RegularExpression("^\\d{8}$",ErrorMessage = "Please try again with valid Card ID number.")]
    public int TargetCardId { get; set; }
    [Required]
    [DefaultValue(50)]
    [RegularExpression("^\\d+$",ErrorMessage = "Please try again with valid amount.")]
    public decimal Amount { get; set; }
}
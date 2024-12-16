using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Application.DTOs;

public class ExchangeMoneyDtoBankAndCard
{
    [Required]
    [RegularExpression("^\\d{8}$",ErrorMessage = "Please try again with valid Card ID number.")]
    public int CardId { get; set; }
    [Required]
    [DefaultValue(50)]
    [RegularExpression("^\\d+$",ErrorMessage = "Please try again with valid amount.")]
    public decimal Amount { get; set; }
}
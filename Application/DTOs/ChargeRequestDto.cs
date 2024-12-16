using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Application.DTOs;

public class ChargeRequestDto
{
    [Required]
    [DefaultValue("pm_card_mastercard")]
    public string PaymentMethod { get; set; }
    [Required]
    [DefaultValue(50)]
    [Range(1,90000,ErrorMessage = $"Accepted range is 1$ up to 90000$")]
    public decimal Amount { get; set; }
}

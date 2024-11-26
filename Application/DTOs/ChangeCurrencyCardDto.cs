using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ChangeCurrencyCardDto
{
    [Required(ErrorMessage = "Card ID required.")]
    [RegularExpression("^\\d{8}",ErrorMessage = "Card IDs consists of 8 digits.")]
    public int CardId { get; set; }
    [Required(ErrorMessage = "Aimed Currency required")]
    [StringLength(3,ErrorMessage = "Accepting currency symbols only.")]
    public string AimedCurrencySymbol { get; set; }
}
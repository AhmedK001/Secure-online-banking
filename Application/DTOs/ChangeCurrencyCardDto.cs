using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ChangeCurrencyCardDto
{
    [Required(ErrorMessage = "Card ID required.")]
    public int CardId { get; set; }
    [Required(ErrorMessage = "Aimed Currency required")]
    [StringLength(3,ErrorMessage = "Accepting currency symbols only.")]
    public string AimedCurrencySymbol { get; set; }
}
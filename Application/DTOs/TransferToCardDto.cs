using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class TransferToCardDto
{
    [Required]
    public int CardId { get; set; }
    [Required]
    public decimal Amount { get; set; }
}
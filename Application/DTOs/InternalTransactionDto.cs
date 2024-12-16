using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class InternalTransactionDto
{
    [Required]
    public int CardId { get; set; }
    [Required]
    [DefaultValue(50)]
    public decimal Amount { get; set; }
}
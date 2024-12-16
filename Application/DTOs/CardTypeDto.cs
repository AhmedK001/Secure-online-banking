using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CardTypeDto
{
    [Required]
    [DefaultValue("PrePaidCard")]
    public string CardType { get; set; }
}
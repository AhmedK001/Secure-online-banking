using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;
public class ConfirmRequestDto
{
    [Required]
    [DefaultValue("pi_xxxxxxxxxxxxxxxxxx")]
    public string PaymentIntentId { get; set; }
}
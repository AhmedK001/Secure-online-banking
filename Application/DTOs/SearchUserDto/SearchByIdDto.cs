using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.SearchUserDto;

public class SearchByIdDto
{
    [Required(ErrorMessage = "National ID cannot be empty.")]
    [RegularExpression(@"^11\d{8}$",
        ErrorMessage
            = "National ID number must consist of exactly 10 digits. Starts with number 11.")]
    public required string NationalId { get; set; }
}
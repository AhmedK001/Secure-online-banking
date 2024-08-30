namespace Core.Interfaces;

public interface ILoginDetails
{
    int NationalId { get; set; }
    string Email { get; set; }
    string Password { get; set; }
    string PhoneNumber { get; set; }
}
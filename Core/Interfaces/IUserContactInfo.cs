namespace Core.Interfaces;

public interface IUserContactInfo
{
    int NationalId { get; set; }
    string Email { get; set; }
    string PhoneNumber { get; set; } 
}
using Core.Interfaces;

namespace Core.Entities;

public class UserContactInfo : IUserContactInfo
{
    public int NationalId { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public User User { get; set; }
}
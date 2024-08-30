using Core.Interfaces;

namespace Core.Entities;

public class LoginDetails : ILoginDetails
{
    public int NationalId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public BankAccount BankAccount { get; set; }
}
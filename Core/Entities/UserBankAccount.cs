using Core.Enums;
using Core.Interfaces;

namespace Core.Entities;

public class UserBankAccount : IBankAccount
{
    public EnumUserRole UserRole { get; set; }
    public int NationalId { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreationDate { get; set; }
}
using Core.Entities;
using Core.Enums;

namespace Core.Interfaces;

public interface IUser
{
     int NationalId { get; set; }
     string FirstName { get; set; }
     string LastName { get; set; }
     string Email { get; set; }
     string PhoneNumber { get; set; } 
     DateTime DateOfBirth { get; set; }
     BankAccount Account { get; set; }
}
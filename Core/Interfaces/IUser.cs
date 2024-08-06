using Core.Enums;

namespace Core.Interfaces;

public interface IUser
{
     EnumUserRole UserRole { get; set; }
     int NationalId { get; set; }
     string FullName { get; set; }
     DateTime DateOfBirth { get; set; }
     string Email { get; set; }
     string PhoneNumber { get; set; } 
}
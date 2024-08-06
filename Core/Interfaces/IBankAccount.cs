namespace Core.Interfaces;

public interface IBankAccount : IUser
{
     int NationalId { get; set; }
     string AccountNumber { get; set; }
     decimal Balance { get; set; }
     DateTime CreationDate { get; set; }
     
}
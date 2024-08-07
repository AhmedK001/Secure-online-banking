namespace Core.Interfaces;

public interface IBankAccount
{
     int NationalId { get; set; }
     string AccountNumber { get; set; }
     decimal Balance { get; set; }
     DateTime CreationDate { get; set; }
     
}
using Core.Entities;

namespace Core.Interfaces;

public interface IBankAccount
{
     int NationalId { get; set; }
     Guid UserId { get; set; }
     string AccountNumber { get; set; }
     decimal Balance { get; set; }
     DateTime CreationDate { get; set; }
     List<Operation> Operations { get; set; }
     
}
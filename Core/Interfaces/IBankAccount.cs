using Core.Entities;
using Core.Enums;

namespace Core.Interfaces;

public interface IBankAccount
{
     int NationalId { get; set; }
     Guid UserId { get; set; }
     string AccountNumber { get; set; }
     public EnumCurrency Currency { get; set; }
     decimal Balance { get; set; }
     DateTime CreationDate { get; set; }
     List<Operation> Operations { get; set; }
     List<Stock> Stocks { get; set; }
}
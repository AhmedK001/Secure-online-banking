using Core.Enums;
using Core.Interfaces;

namespace Core.Entities;

public class Transaction : ITransaction
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; }
    public int TransactionId { get; set; }
    public DateTime DateTime { get; set; }
    private EnumTransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    
    public ITransaction GetTransactionInfo()
    {
        throw new NotImplementedException();
    }
}
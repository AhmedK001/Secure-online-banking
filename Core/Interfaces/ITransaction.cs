namespace Core.Interfaces;

public interface ITransaction
{
    int AccountId { get; set; }
    string AccountNumber { get; set; }
    int TransactionId { get; set; }
    DateTime DateTime { get; set; }
    decimal Amount { get; set; }

    ITransaction GetTransactionInfo();
}
using Core.Entities;
using Core.Enums;

namespace Application.Interfaces;

public interface IOperationService
{
    bool IsValidOperationObject(Operation? operation);
    Task<int> GenerateUniqueRandomOperationIdAsync();
    Task AddOperation(bool saveAsync, Operation operation);
    Task<bool> ValidateAndSaveOperation(Operation operation);
    Task<List<Operation>> GetCurrencyChangeLogs(string accountNumber);
    Task<List<Operation>> GetChargeAccountLogs(string accountNumber);
    Task<List<Operation>> GetTransactionsToCardLogs(string accountNumber);
    Task<List<Operation>> GetAllLogs(string accountNumber, int periodAsMonth);
    Task<Operation> BuildChargeOperation(BankAccount account, decimal amount);
    Task<Operation> BuildTransferOperation(BankAccount account, decimal amount,EnumOperationType type);

    Task<Operation> BuildStockOperation(BankAccount account, int number, string symbol, string corpName, decimal price,
        EnumOperationType operationType);

    Task<Operation> BuildDeleteOrCreateCardOperation(BankAccount account, Card card, EnumOperationType operationType);

    Task<Operation> BuildExchangeOperation(BankAccount? account, Card? baseCard, Card? targetCard, decimal amount,
        EnumOperationType type);

}
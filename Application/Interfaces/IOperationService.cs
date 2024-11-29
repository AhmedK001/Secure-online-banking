using Core.Entities;

namespace Application.Interfaces;

public interface IOperationService
{
    bool IsValidOperationObject(Operation? operation);
    Task<int> GenerateUniqueRandomOperationIdAsync();
    Task LogOperation(bool saveAsync,Operation operation);
    Task<bool> AddAndSaveOperation(Operation operation);
    Task<List<Operation>> GetCurrencyChangeLogs(string accountNumber);
    Task<List<Operation>> GetChargeAccountLogs(string accountNumber);
    Task<List<Operation>> GetTransactionsToCardLogs(string accountNumber);
}
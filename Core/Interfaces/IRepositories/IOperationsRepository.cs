using Core.Entities;

namespace Core.Interfaces.IRepositories;

public interface IOperationsRepository
{
    Task<bool> IsOperationIdUnique(int operationId);
    Task AddOperation(bool saveAsync,Operation operation);
    Task<List<Operation>> GetCurrencyChangeLogs(string accountNumber);
    Task<List<Operation>> GetChargeAccountLogs(string accountNumber);
    Task<List<Operation>> GetTransactionsToCardLogs(string accountNumber);
}
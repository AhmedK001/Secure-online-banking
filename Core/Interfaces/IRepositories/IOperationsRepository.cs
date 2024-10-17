using Core.Entities;

namespace Core.Interfaces.IRepositories;

public interface IOperationsRepository
{
    Task<bool> IsOperationIdUnique(int operationId);
    Task AddOperation(Operation operation);
}
using Core.Entities;

namespace Application.Interfaces;

public interface IOperationService
{
    bool IsValidOperationObject(Operation? operation);
    Task<int> GenerateUniqueRandomOperationIdAsync();
    Task AddOperation(Operation operation);
}
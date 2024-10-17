using Application.Interfaces;
using Core.Entities;
using Core.Interfaces.IRepositories;

namespace Application.Services;


public class OperationService : IOperationService
{
    private readonly IOperationsRepository _operationsRepository;

    public OperationService(IOperationsRepository operationsRepository)
    {
        _operationsRepository = operationsRepository;
    }

    public bool IsValidOperationObject (Operation? operation)
    {
        if (operation == null)
            return false;

        if (string.IsNullOrWhiteSpace(operation.AccountNumber))
            return false;

        if (operation.OperationId <= 0)
            return false;

        if (operation.AccountId <= 0)
            return false;

        if (operation.DateTime == DateTime.MinValue)
            return false;

        if (operation.Amount <= 0)
            return false;

        return true;
    }

    public async Task AddOperation(Operation operation)
    {
        await _operationsRepository.AddOperation(operation);
    }

    public async Task<int> GenerateUniqueRandomOperationIdAsync()
    {
        Random random = new Random();
        int randomId;

        do
        {
            randomId = random.Next(10000000, 99999999); // Random number between 100000 and 999999
        }
        while (!await _operationsRepository.IsOperationIdUnique(randomId));

        return randomId; // Return unique random ID
    }

    public async Task<Operation> ConvertToOperationObjectAsync()
    {
        throw new NotImplementedException();
    }

}
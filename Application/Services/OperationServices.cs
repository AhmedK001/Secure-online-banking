using Application.Interfaces;
using Core.Entities;
using Core.Interfaces.IRepositories;

namespace Application.Services;


public class OperationServices : IOperationService
{
    private readonly IOperationsRepository _operationsRepository;

    public OperationServices(IOperationsRepository operationsRepository)
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

        if (operation.DateTime == DateTime.MinValue)
            return false;

        if (operation.Amount <= 0)
            return false;

        return true;
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

    public async Task LogOperation(bool saveAsync,Operation operation)
    {
        await _operationsRepository.AddOperation(saveAsync,operation);
    }

    public async Task<bool> AddAndSaveOperation(Operation operation)
    {
        try
        {
            if (!IsValidOperationObject(operation))
            {
                throw new Exception("Operation object not valid");
            }

            await _operationsRepository.AddOperation(true,operation);

            return true;
        }
        catch (Exception e)
        {
            throw new Exception();
        }

    }

    public async Task<List<Operation>> GetCurrencyChangeLogs(string accountNumber)
    {
        try
        {
            return await _operationsRepository.GetCurrencyChangeLogs(accountNumber);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<List<Operation>> GetChargeAccountLogs(string accountNumber)
    {
        try
        {
            return await _operationsRepository.GetChargeAccountLogs(accountNumber);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<List<Operation>> GetTransactionsToCardLogs(string accountNumber)
    {
        try
        {
            return await _operationsRepository.GetTransactionsToCardLogs(accountNumber);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }
}
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OperationsRepository : IOperationsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OperationsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsOperationIdUnique(int operationId)
    {
        if (!await _dbContext.Operations.AnyAsync(o => o.OperationId == operationId))
        {
            return true;
        }

        return false;
    }

    public async Task AddOperation(bool saveAsync,Operation operation)
    {
        try
        {
            var addOperationAsync
                = await _dbContext.Operations.AddAsync(operation);
            if (saveAsync)
            {
                await _dbContext.SaveChangesAsync();
            }
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
            var result = await _dbContext.Operations
                .Where(o => o.AccountNumber == accountNumber)
                .Where(o => o.OperationType == EnumOperationType.CurrencyChange)
                .ToListAsync();
            return result;
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
            var result = await _dbContext.Operations
                .Where(o => o.AccountNumber == accountNumber)
                .Where(o => o.OperationType == EnumOperationType.Deposit)
                .ToListAsync();
            return result;
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
            var result = await _dbContext.Operations
                .Where(o => o.AccountNumber == accountNumber)
                .Where(o => o.OperationType == EnumOperationType.TransactionToCard)
                .ToListAsync();
            return result;
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }
}
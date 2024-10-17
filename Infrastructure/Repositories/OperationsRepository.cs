using Core.Entities;
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

    public async Task AddOperation(Operation operation)
    {
        try
        {
            var addOperationAsync
                = await _dbContext.Operations.AddAsync(operation);
        }
        catch (Exception e)
        {
            throw new Exception();
        }

        await _dbContext.SaveChangesAsync();
    }
}
using Core.Entities;
using Core.Interfaces.IRepositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StockRepository : IStockRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StockRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> SaveStock(Stock stock)
    {
        try
        {
            await _dbContext.Stocks.AddAsync(stock);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<List<Stock>> GetAllStocks(string accountNumber)
    {
        try
        {
            return await _dbContext.Stocks.Where(s => s.AccountNumber == accountNumber).ToListAsync();
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<List<Stock>> GetStocksBySymbol(string accountNumber, string symbol)
    {
        try
        {
            return await _dbContext.Stocks.Where(s => s.StockSymbol == symbol && s.AccountNumber == accountNumber)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<int> GetStockSumByType(string accountNumber, string symbol)
    {
        try
        {
            return await _dbContext.Stocks.Where(s => s.StockSymbol == symbol && s.AccountNumber == accountNumber)
                .SumAsync(s => s.NumberOfStocks);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task RemoveStock(Stock stock)
    {
        try
        {
            _dbContext.Stocks.Remove(stock);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task UpdateStock(Stock stock)
    {
        try
        {
            _dbContext.Stocks.Update(stock);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
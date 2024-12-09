using Core.Entities;

namespace Core.Interfaces.IRepositories;

public interface IStockRepository
{
    Task<bool> SaveStock(Stock stock);
    Task<List<Stock>> GetAllStocks(string accountNumber);
    Task<List<Stock>> GetStocksBySymbol(string accountNumber, string symbol);
    Task<int> GetStockSumByType(string accountNumber, string symbol);
    Task RemoveStock(Stock stock);
    Task UpdateStock(Stock stock);

}
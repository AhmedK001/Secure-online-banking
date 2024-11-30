using Core.Entities;

namespace Core.Interfaces.IRepositories;

public interface IStockRepository
{
    Task<bool> SaveStock(Stock stock);
    Task<List<Stock>> GetAllStocks(string accountNumber);
}
using Core.Entities;
using Core.Enums;

namespace Core.Interfaces;

public interface IStock
{
    int StockId { get; set; }
    string AccountNumber { get; set; }
    string StockName { get; set; }
    string StockSymbol { get; set; }
    decimal StockPrice { get; set; }
    int NumberOfStocks { get; set; }
    DateTime DateOfPurchase { get; set; }
    EnumCurrency Currency { get; set; }
    BankAccount BankAccount { get; set; }
}
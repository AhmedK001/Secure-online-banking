using Core.Enums;
using Core.Interfaces;

namespace Core.Entities;

public class Stock : IStock
{
    public int StockId { get; set; }
    public string AccountNumber { get; set; }
    public string StockName { get; set; }
    public string StockSymbol { get; set; }
    public decimal StockPrice { get; set; }
    public int NumberOfStocks { get; set; }
    public DateTime DateOfPurchase { get; set; }
    public EnumCurrency Currency { get; set; }
    public BankAccount BankAccount { get; set; }
}
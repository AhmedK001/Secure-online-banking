using System.Text.Json.Nodes;
using Application.DTOs;
using Application.DTOs.ExternalModels;
using Application.DTOs.ExternalModels.Finnhub;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Core.Entities;

namespace Application.Interfaces;

public interface IStockService
{
    Task<StockPriceFinnhubResponse> GetStockLivePrice(string symbol);
    Task<JsonObject> GetStockPricesAsync(StockPricesDto stockPricesDto);
    Task<List<TopGainers>> GetTopGainers();
    Task<List<TopLosers>> GetTopLosers();
    Task<List<MostActivelyTraded>> GetMostActivelyStocks();
    Task<StockPriceFinnhubResponse> DeserializeStockPriceFinnhubResponse(JsonObject jsonObject);
    Task<(bool,string)> BuyStockAsync(BankAccount bankAccount,StockPriceFinnhubResponse priceResponse,StockLookUpResponse detailsResponse, BuySellStockDto buyStockDto);
    Task<StockLookUpResponse> GetStockDetails(string symbol);
    Task<List<Stock>> GetAllStocks(string accountNumber);
    Task<List<Stock>> GetStocksBySymbol(string accountNumber, string symbol);

    Task<(bool, string)> SellStockAsync(BankAccount bankAccount, BuySellStockDto stockDto);

}
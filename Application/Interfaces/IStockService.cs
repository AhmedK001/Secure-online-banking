using System.Text.Json.Nodes;
using Application.DTOs;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;

namespace Application.Interfaces;

public interface IStockService
{


    Task<JsonObject> GetStockPricesAsync(StockPricesDto stockPricesDto);
    Task<List<TopGainers>> GetTopGainers();
    Task<List<TopLosers>> GetTopLosers();
    Task<List<MostActivelyTraded>> GetMostActivelyStocks();
}
using System.Text.Json.Nodes;
using Application.DTOs;

namespace Application.Interfaces;

public interface IStockService
{


    Task<JsonObject> GetStockPricesAsync(StockPricesDto stockPricesDto);
    Task<JsonObject> GetTopGainersAndLosersAsync();
}
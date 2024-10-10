using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;
    private static readonly string ApiKey = "LKMNW304JYH21FWO";
    private static readonly string DemoApiKey = "demo";

    public StockService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JsonObject> GetStockPricesAsync(StockPricesDto stockPricesDto)
    {
        var requistUri = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={stockPricesDto.Symbol}&interval={stockPricesDto.Timestamp}min&apikey={ApiKey}";

        var response = await _httpClient.GetAsync(requistUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new KeyNotFoundException("Stock prices not found.");
        }

        var jsonResponse = await response.Content.ReadAsStreamAsync();
        var jsonNode = await JsonNode.ParseAsync(jsonResponse);

        if (jsonNode is JsonObject jsonObject)
        {
            // return if successes
            return jsonObject;
        }

        throw new KeyNotFoundException("Stock prices not found.");
    }

    public async Task<JsonObject> GetTopGainersAndLosersAsync()
    {
        var requistUri
            = $"https://www.alphavantage.co/query?function=TOP_GAINERS_LOSERS&apikey={DemoApiKey}";

        var response = await _httpClient.GetAsync(requistUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new KeyNotFoundException("Something went wrong");
        }

        var jsonResponse = await response.Content.ReadAsStreamAsync();
        var jsonNode = await JsonNode.ParseAsync(jsonResponse);

        if (jsonNode is JsonObject jsonObject)
        {
            return jsonObject;
        }

        throw new KeyNotFoundException();
    }
}
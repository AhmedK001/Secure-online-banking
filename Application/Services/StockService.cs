using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Application.DTOs;
using Application.DTOs.ExternalModels.StocksApiResponse.GetSingleStock;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;


    public StockService(HttpClient httpClient,IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /// <summary>
    /// This method request external api getting one stock prices related to some interval that user choose
    /// </summary>
    /// <param name="stockPricesDto"></param>
    /// <returns>one stock prices</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<JsonObject> GetStockPricesAsync(StockPricesDto stockPricesDto)
    {
        var requestUri = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={stockPricesDto.Symbol}&interval={stockPricesDto.Timestamp}min&apikey={_configuration["AlphaVantageApi:ApiKey"]}";

        var response = await _httpClient.GetAsync(requestUri);

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

    /// <summary>
    /// This method is responsible to Deserialize stock prices details from jsonObject to  StockPriceDetails object
    /// </summary>
    /// <param name="jsonObject"></param>
    /// <returns>StockPriceDetails object</returns>
    /// <exception cref="JsonException"></exception>
    private async Task<StockPriceDetails> DeserializeStockPriceDetails(JsonObject jsonObject)
    {
        if (jsonObject is null)
        {
            throw new JsonException();
        }

        var jsonResponse = jsonObject.ToJsonString();

        StockPriceDetails stockPriceDetails = JsonSerializer.Deserialize<StockPriceDetails>(jsonResponse);

        if (stockPriceDetails == null)
        {
            throw new JsonException();
        }

        return stockPriceDetails;
    }

    /// <summary>
    /// This method request external api getting Top Gainers & Top Losers & Most Actively stocks
    /// </summary>
    /// <returns>Mentioned data as Json Object </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    private async Task<JsonObject> GetTopGainersAndLosersAndActivityAsync()
    {
        var requistUri
            = $"https://www.alphavantage.co/query?function=TOP_GAINERS_LOSERS&apikey={_configuration["AlphaVantageApi:ApiKey"]}";

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

    /// <summary>
    /// This method responsible for deserializing jsonObject to GainersLosersActiveResponse object
    /// </summary>
    /// <param name="jsonObject"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    private async Task<GainersLosersActive> DeserializeGainersLosersActiveResponse(JsonObject jsonObject)
    {
        var jsonResponse = jsonObject.ToJsonString();

        Console.WriteLine(jsonResponse);

        GainersLosersActive? gainersLosersActive = JsonSerializer.Deserialize<GainersLosersActive>(jsonResponse);

        if (gainersLosersActive == null)
        {
            throw new JsonException();
        }

        return gainersLosersActive;
    }

    public async Task<List<TopGainers>> GetTopGainers()
    {
        GainersLosersActive gainersLosersActive = await DeserializeGainersLosersActiveResponse(await GetTopGainersAndLosersAndActivityAsync());

        return gainersLosersActive.TopGainers;
    }

    public async Task<List<TopLosers>> GetTopLosers()
    {
        GainersLosersActive gainersLosersActive = await DeserializeGainersLosersActiveResponse(GetTopGainersAndLosersAndActivityAsync().Result);

        return gainersLosersActive.TopLosers;
    }

    public async Task<List<MostActivelyTraded>> GetMostActivelyStocks()
    {
        GainersLosersActive gainersLosersActive = await DeserializeGainersLosersActiveResponse(GetTopGainersAndLosersAndActivityAsync().Result);

        return gainersLosersActive.MostActivelyStocks;
    }
}
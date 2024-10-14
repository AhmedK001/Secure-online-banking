using System.Text.Json;
using System.Text.Json.Nodes;
using Application.DTOs.ExternalModels.Currency;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;


    public CurrencyService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<JsonObject> GetExchangeRate(string currentCurrency, string aimedCurrency)
    {
        var apiKey = _configuration["AlphaVantageApi:ApiKey"];

        string requestUri
            = $"https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency={currentCurrency}&to_currency={aimedCurrency}&apikey={apiKey}";
        var response = await _httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException();
        }

        var streamResponse = await response.Content.ReadAsStreamAsync();
        var jsonNode = await JsonNode.ParseAsync(streamResponse);

        if (jsonNode is JsonObject jsonObject)
        {
            // return if successes
            return jsonObject;
        }

        throw new JsonException();
    }

    private async Task<ExchangeRateDto> DeserializeExchangeRate(JsonObject jsonString)
    {
        var jsonResponse = jsonString.ToJsonString();

        Console.WriteLine(jsonResponse);

        ExchangeRateDto? exchangeRateDto = JsonSerializer.Deserialize<ExchangeRateDto>(jsonResponse);

        if (exchangeRateDto == null)
        {
            throw new JsonException();
        }

        return exchangeRateDto;
    }

    public async Task<JsonObject> GetHistoricalExchangeRate(string currentCurrency, string aimedCurrency,
        string timeSeries)
    {
        var requestUri = await GetRequestUriForHistoricalExchangeRate(currentCurrency, aimedCurrency, timeSeries);
        var response = await _httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException();
        }

        var streamResponse = await response.Content.ReadAsStreamAsync();
        var jsonNode = await JsonNode.ParseAsync(streamResponse);

        if (jsonNode is JsonObject jsonObject)
        {
            // return if successes
            return jsonObject;
        }

        throw new JsonException();
    }

    private async Task<string> GetRequestUriForHistoricalExchangeRate(string currentCurrency, string aimedCurrency,
        string timeSeries)
    {
        var apiKey = _configuration["AlphaVantageApi:ApiKey"];
        string requestUri
            = $"https://www.alphavantage.co/query?function=FX_DAILY&from_symbol={currentCurrency}&to_symbol={aimedCurrency}&apikey={apiKey}";

        if (timeSeries.Equals("daily", StringComparison.OrdinalIgnoreCase))
        {
            requestUri
                = $"https://www.alphavantage.co/query?function=FX_{timeSeries.ToUpper()}&from_symbol={currentCurrency}&to_symbol={aimedCurrency}&apikey={apiKey}";
            return requestUri;
        }

        if (timeSeries.Equals("weekly", StringComparison.OrdinalIgnoreCase))
        {
            requestUri
                = $"https://www.alphavantage.co/query?function=FX_{timeSeries.ToUpper()}&from_symbol={currentCurrency}&to_symbol={aimedCurrency}&apikey={apiKey}";
            return requestUri;
        }

        if (timeSeries.Equals("monthly", StringComparison.OrdinalIgnoreCase))
        {
            requestUri
                = $"https://www.alphavantage.co/query?function=FX_{timeSeries.ToUpper()}&from_symbol={currentCurrency}&to_symbol={aimedCurrency}&apikey={apiKey}";
            return requestUri;
        }

        return requestUri;
    }
}
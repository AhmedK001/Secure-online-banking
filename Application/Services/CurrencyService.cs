using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Application.DTOs.ExternalModels.Currency;
using Application.Interfaces;
using Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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

    public async Task<JsonObject?> FetchExchangeRate(EnumCurrency currentCurrency, EnumCurrency aimedCurrency)
    {
        var apiKey = _configuration["AlphaVantageApi:ApiKey"];

        string requestUri
            = $"https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency={currentCurrency}&to_currency={aimedCurrency}&apikey={apiKey}";
        var response = await _httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var streamResponse = await response.Content.ReadAsStreamAsync();
        var jsonNode = await JsonNode.ParseAsync(streamResponse);

        if (jsonNode is JsonObject jsonObject)
        {
            if (jsonObject.TryGetPropertyValue("Error Message", out var errorMessageNode) && errorMessageNode != null)
            {
                // string errorMessage = errorMessageNode.ToString();
                // throw new Exception($"Alpha Vantage API error: {errorMessage}");
                return null;
            }
            // return if successes
            return jsonObject;
        }

        throw new JsonException();
    }

    public async Task<(bool isFirstChance, JsonObject data)> GetExchangeRate(EnumCurrency currentCurrency, EnumCurrency aimedCurrency)
    {
        try
        {
            var exchangeRate = await FetchExchangeRate(currentCurrency, aimedCurrency);

            if (!exchangeRate.IsNullOrEmpty())
            {
                return (true,exchangeRate);
            }

            // changed order of current & aimed currencies because it is one way API
            exchangeRate = await FetchExchangeRate(aimedCurrency, currentCurrency);

            if (!exchangeRate.IsNullOrEmpty())
            {
                return (false,exchangeRate);
            }

            throw new Exception("Could not get response from the External Api service.");
        }
        catch (Exception e)
        {
            throw new Exception("",e);
        }
    }

    private async Task<ExchangeRateDto> DeserializeExchangeRate(JsonObject jsonString)
    {
        var jsonResponse = jsonString.ToString();

        Console.WriteLine(jsonResponse);

        var response = JsonSerializer.Deserialize<CurrencyExchangeRateResponse>(jsonResponse);

        if (string.IsNullOrEmpty(response.ToString()))
        {
            throw new JsonException();
        }

        return response.ExchangeRateDto;
    }

    public async Task<JsonObject> GetHistoricalExchangeRate(EnumCurrency currentCurrency, EnumCurrency aimedCurrency,
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

    public async Task<ExchangeRateDto> GetExchangeForm(EnumCurrency fromCurrency, EnumCurrency toCurrency)
    {
        try
        {
            ExchangeRateDto exchangeRateDto;
            var exchangeRateResult = await GetExchangeRate(fromCurrency, toCurrency);

            // Handles one way exchange.
            if (exchangeRateResult.isFirstChance != true)
            {
                exchangeRateDto = await GetOppositeExchangeRate(exchangeRateResult.data);
            }
            else
            {
                exchangeRateDto = await DeserializeExchangeRate(exchangeRateResult.data);
            }

            return exchangeRateDto;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<ExchangeRateDto> GetOppositeExchangeRate(JsonObject exchangeRateResult)
    {
        var exchangeRateObject = await DeserializeExchangeRate(exchangeRateResult);
        (exchangeRateObject.FromCurrencyCode, exchangeRateObject.ToCurrencyCode) = (
            exchangeRateObject.ToCurrencyCode, exchangeRateObject.FromCurrencyCode);
        (exchangeRateObject.FromCurrencyName, exchangeRateObject.ToCurrencyName) = (
            exchangeRateObject.ToCurrencyName, exchangeRateObject.FromCurrencyName);

        exchangeRateObject.AskPrice = (1 / decimal.Parse(exchangeRateObject.AskPrice)).ToString();
        exchangeRateObject.BidPrice = (1 / decimal.Parse(exchangeRateObject.BidPrice)).ToString();

        return (exchangeRateObject);
    }

    private async Task<string> GetRequestUriForHistoricalExchangeRate(EnumCurrency currentCurrency, EnumCurrency aimedCurrency,
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
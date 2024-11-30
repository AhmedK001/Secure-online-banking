using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Application.DTOs;
using Application.DTOs.ExternalModels;
using Application.DTOs.ExternalModels.Finnhub;
using Application.DTOs.ExternalModels.StocksApiResponse.GetSingleStock;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IOperationService _operationService;
    private readonly IStockRepository _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankAccountService _bankAccountService;


    public StockService(IBankAccountService bankAccountService,IUnitOfWork unitOfWork,HttpClient httpClient,IConfiguration configuration, IOperationService operationService, IStockRepository stockRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _operationService = operationService;
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _bankAccountService = bankAccountService;
    }

    public async Task<StockPriceFinnhubResponse> GetStockLivePrice(string symbol)
    {
        try
        {
            var apiKey = _configuration["Finnhub:ApiKey"];

            var uri = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={apiKey}";

            var request = await _httpClient.GetAsync(uri);

            if (!request.IsSuccessStatusCode)
            {
                throw new Exception("Error happened while sending request.");
            }

            var streamResponse = await request.Content.ReadAsStreamAsync();

            var jsonNodeResponse = await JsonNode.ParseAsync(streamResponse);

            if (jsonNodeResponse is not JsonObject jsonObject)
            {
                throw new JsonException();
            }

            return await DeserializeStockPriceFinnhubResponse(jsonObject);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<StockPriceFinnhubResponse> DeserializeStockPriceFinnhubResponse(JsonObject jsonObject)
    {
        try
        {
            var jsonString = jsonObject.ToString();
            return JsonSerializer.Deserialize<StockPriceFinnhubResponse>(jsonString);
        }
        catch (Exception e)
        {
            throw new Exception("No stock found.");
        }
    }

    public async Task<(bool,string)> BuyStockAsync(BankAccount bankAccount,StockPriceFinnhubResponse priceResponse, StockLookUpResponse detailsResponse, BuyStockDto buyStockDto)
    {
        try
        {
            if (priceResponse is null || detailsResponse is null)
            {
                throw new Exception("Invalid objects");
            }

            if (bankAccount.Currency != EnumCurrency.USD)
            {
                throw new Exception("Your bank account currency must be in Dollar.");
            }

            Stock stock = new Stock()
            {
                AccountNumber = bankAccount.AccountNumber,
                StockId = await _operationService.GenerateUniqueRandomOperationIdAsync(),
                StockName = detailsResponse.Result[0].Description,
                StockSymbol = detailsResponse.Result[0].Symbol,
                NumberOfStocks = buyStockDto.NumberOfStocks,
                StockPrice = priceResponse.CurrentPrice,
                Currency = EnumCurrency.USD,
                DateOfPurchase = DateTime.UtcNow
            };

            var totalPrice = priceResponse.CurrentPrice * buyStockDto.NumberOfStocks;

            if (totalPrice > bankAccount.Balance)
            {
                return (false,
                    $"No enough balance, Your balance is {bankAccount.Balance}{bankAccount.Currency}");
            }

            await _unitOfWork.BeginTransactionAsync();
            await _stockRepository.SaveStock(stock);
            await _bankAccountService.DeductAccountBalance(bankAccount.AccountNumber, totalPrice);

            await _unitOfWork.CommitTransactionAsync();

            return (true,$"");
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new Exception(e.Message);
        }
    }

    public async Task<StockLookUpResponse> GetStockDetails(string symbol)
    {
        try
        {
            var apiKey = _configuration["Finnhub:ApiKey"];

            var uri = $"https://finnhub.io/api/v1/search?q={symbol}&exchange=US&token={apiKey}";

            var request = await _httpClient.GetAsync(uri);

            if (!request.IsSuccessStatusCode)
            {
                throw new Exception("Error happened while sending request.");
            }

            var streamResponse = await request.Content.ReadAsStreamAsync();

            var jsonNodeResponse = await JsonNode.ParseAsync(streamResponse);

            if (jsonNodeResponse is not JsonObject jsonObject)
            {
                throw new JsonException();
            }

            return JsonSerializer.Deserialize<StockLookUpResponse>(jsonObject);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<Stock>> GetAllStocks(string accountNumber)
    {
        try
        {
            return await _stockRepository.GetAllStocks(accountNumber);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
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
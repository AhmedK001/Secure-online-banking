using System.Text.Json.Nodes;
using Application.DTOs;
using Application.DTOs.ExternalModels;
using Application.DTOs.ExternalModels.Finnhub;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Web.Controllers;

[ApiController]
[Route("api/stocks")]
public class StocksController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly IClaimsService _claimsService;
    private readonly IBankAccountService _bankAccountService;
    private readonly IEmailService _emailService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;
    private readonly IConfiguration _configuration;
    private readonly string _sendGridApiKey;
    private readonly string _email;
    private readonly ILogger<StocksController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IOperationService _operationService;
    private readonly IMemoryCache _memoryCache;

    public StocksController(IMemoryCache memoryCache,IOperationService operationService, UserManager<User> userManager,
        ILogger<StocksController> logger, IConfiguration configuration, IEmailBodyBuilder emailBodyBuilder,
        IEmailService emailService, IStockService stockService, IClaimsService claimsService,
        IBankAccountService bankAccountService)
    {
        _stockService = stockService;
        _claimsService = claimsService;
        _bankAccountService = bankAccountService;
        _emailService = emailService;
        _emailBodyBuilder = emailBodyBuilder;
        _sendGridApiKey = configuration["SendGrid:ApiKey"];
        _email = configuration["Email"];
        _logger = logger;
        _userManager = userManager;
        _operationService = operationService;
        _memoryCache = memoryCache;
    }

    [HttpGet("owned")]
    [Authorize]
    public async Task<IActionResult> GetOwnedStocks()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var stocks = await _stockService.GetAllStocks(bankAccountDetails.AccountNumber);


            var stocksResponse = stocks.Select(s => new
            {
                StockName = s.StockName,
                StockSymbol = s.StockSymbol,
                StockId = s.StockId,
                DateOfPurchase = s.DateOfPurchase.ToString("g"),
                StockPrice = s.StockPrice,
                NumberOfStocks = s.NumberOfStocks,
                TotalAmountSpent = s.NumberOfStocks * s.StockPrice,
                Currency = Enum.GetName(typeof(EnumCurrency), s.Currency),
            }).ToList();

            if (!stocksResponse.Any())
            {
                return Ok(new { Message = "No stocks found." });
            }

            return Ok(new { stocksResponse });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("buy")]
    [Authorize]
    public async Task<IActionResult> BuyStock([FromQuery] BuySellStockDto sellStockDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            var stockPrice = await _stockService.GetStockLivePrice(sellStockDto.Symbol);
            var stockDetails = await _stockService.GetStockDetails(sellStockDto.Symbol);
            var buyStockResult
                = await _stockService.BuyStockAsync(bankAccountDetails, stockPrice, stockDetails, sellStockDto);
            if (!buyStockResult.Item1)
            {
                return BadRequest(new { Message = buyStockResult.Item2 });
            }

            var response = new
            {
                Message = "Operation done successfully!",
                StockName = stockDetails.Result[0].Description,
                stockDetails.Result[0].Symbol,
                stockPrice.CurrentPrice,
                sellStockDto.NumberOfStocks
            };

            // save operation
            var operation = await _operationService.BuildStockOperation(bankAccountDetails, sellStockDto.NumberOfStocks,
                sellStockDto.Symbol, stockDetails.Result[0].DisplaySymbol, stockPrice.CurrentPrice,
                EnumOperationType.StockBuy);
            await _operationService.ValidateAndSaveOperation(operation);

            var totalPrice = stockPrice.CurrentPrice * sellStockDto.NumberOfStocks;

                var responseHtml = _emailBodyBuilder.BuyStockHtmlResponse("You have successfully purchased stocks",
                    stockDetails.Result[0].Description, stockDetails.Result[0].Symbol, stockPrice.CurrentPrice,
                    sellStockDto.NumberOfStocks, totalPrice);

                await _emailService.SendEmailAsync(user, "You have successfully purchased stocks", responseHtml);

            return Ok(new
            {
                Message = "Operation done successfully!",
                StockName = stockDetails.Result[0].Description,
                stockDetails.Result[0].Symbol,
                stockPrice.CurrentPrice,
                sellStockDto.NumberOfStocks
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("sell")]
    [Authorize]
    public async Task<IActionResult> SellStock([FromQuery] BuySellStockDto sellStockDto)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));

            if (bankAccountDetails.Currency != EnumCurrency.USD)
            {
                return BadRequest("You bank account currency must be in Dollar, in order to Sell owned Stocks");
            }

            var stocks = await _stockService.GetAllStocks(bankAccountDetails.AccountNumber);

            var result = await _stockService.SellStockAsync(bankAccountDetails, sellStockDto);
            if (!result.Item1)
            {
                return Ok(new { ErrorMessage = result.Item2 });
            }

            var livePrice = await _stockService.GetStockLivePrice(sellStockDto.Symbol);
            var info = await _stockService.GetStockDetails(sellStockDto.Symbol);

            var operation = await _operationService.BuildStockOperation(bankAccountDetails, sellStockDto.NumberOfStocks,
                sellStockDto.Symbol, info.Result[0].DisplaySymbol, livePrice.CurrentPrice,
                EnumOperationType.StockSell);
            await _operationService.ValidateAndSaveOperation(operation);

            return Ok(new { Message = "Done successfully." });
        }
        catch (Exception e)
        {
            return BadRequest(new { e.Message });
        }
    }

    [HttpGet("live-price")]
    [Authorize]
    public async Task<IActionResult> GetStockLivePrice([FromQuery] CurrencySymbolDto currencySymbolDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StockPriceFinnhubResponse result;
            string key = $"{currencySymbolDto.Symbol}";

            if (_memoryCache.TryGetValue(key,out StockPriceFinnhubResponse? data))
            {
                result = data;
            }
            else
            {
                result = await _stockService.GetStockLivePrice(currencySymbolDto.Symbol);
                _memoryCache.Set(key, result, TimeSpan.FromMinutes(2));
            }

            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("historical")]
    [Authorize]
    public async Task<IActionResult> GetStockPrices([FromQuery] StockPricesDto stockPricesDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }


        JsonObject stockPrices;
        string key = $"{stockPricesDto.Symbol}{stockPricesDto.Timestamp}";

        if (_memoryCache.TryGetValue(key,out JsonObject? data))
        {
            stockPrices = data;
        }
        else
        {
            stockPrices = await _stockService.GetStockPricesAsync(stockPricesDto);

            if (stockPrices.IsNullOrEmpty())
            {
                return BadRequest("Something went wrong.");
            }

            _memoryCache.Set(key, stockPrices, TimeSpan.FromMinutes(stockPricesDto.Timestamp));
        }

        return Ok(stockPrices);
    }

    [HttpGet("top-gainers")]
    [Authorize]
    public async Task<IActionResult> GetTopGainers()
    {

        string key = "topGainers";
        List<TopGainers> topGainersResponse;
        if (_memoryCache.TryGetValue(key, out List<TopGainers>? gainersList))
        {
            topGainersResponse = gainersList;
        }
        else
        {
            topGainersResponse = await _stockService.GetTopGainers();

            if (!topGainersResponse.Any())
            {
                return BadRequest("Something went wrong while fetching data from the Provider.");
            }

            _memoryCache.Set(key, topGainersResponse, TimeSpan.FromMinutes(10));
        }

        return Ok(topGainersResponse);
    }

    [HttpGet("top-losers")]
    [Authorize]
    public async Task<IActionResult> GetTopLosers()
    {

        List<TopLosers> topLosersResponse;
        string key = "topLosers";
        if (_memoryCache.TryGetValue(key, out List<TopLosers>? topLosers))
        {
            topLosersResponse = topLosers;
        }
        else
        {
            topLosersResponse = await _stockService.GetTopLosers();

            if (!topLosersResponse.Any())
            {
                return BadRequest("Something went wrong while fetching data from the Provider.");
            }

            _memoryCache.Set(key, topLosersResponse, TimeSpan.FromMinutes(10));
        }

        return Ok(topLosersResponse);
    }

    [HttpGet("most-actively")]
    [Authorize]
    public async Task<IActionResult> GetMostActively()
    {
        List<MostActivelyTraded> mostActively;
        string key = "mostActively";

        if (_memoryCache.TryGetValue(key, out List<MostActivelyTraded>? mostActivelyData))
        {
            mostActively = mostActivelyData;
        }
        else
        {
            mostActively = await _stockService.GetMostActivelyStocks();

            if (!mostActively.Any())
            {
                return BadRequest("Something went wrong while fetching data from the Provider.");
            }

            _memoryCache.Set(key, mostActively, TimeSpan.FromMinutes(10));
        }

        return Ok(mostActively);
    }
}
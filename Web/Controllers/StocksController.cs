using Application.DTOs;
using Application.DTOs.ExternalModels;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public StocksController(IEmailService emailService,IStockService stockService, IClaimsService claimsService, IBankAccountService bankAccountService)
    {
        _stockService = stockService;
        _claimsService = claimsService;
        _bankAccountService = bankAccountService;
        _emailService = emailService;
    }

    [HttpPost("buy-stock")]
    [Authorize]
    public async Task<IActionResult> BuyStock([FromQuery] BuyStockDto stockDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var stockPrice = await _stockService.GetStockLivePrice(stockDto.Symbol);
            var stockDetails = await _stockService.GetStockDetails(stockDto.Symbol);
            var buyStockResult = await _stockService.BuyStockAsync(bankAccountDetails, stockPrice, stockDetails, stockDto);
            if (!buyStockResult.Item1)
            {
                return BadRequest(new
                {
                    Message = buyStockResult.Item2
                });
            }

            var response = new
            {
                Message = "Operation done successfully!",
                StockName = stockDetails.Result[0].Description,
                stockDetails.Result[0].Symbol,
                stockPrice.CurrentPrice,
                stockDto.NumberOfStocks
            };

            return Ok(new
            {
                Message = "Operation done successfully!",
                StockName = stockDetails.Result[0].Description,
                stockDetails.Result[0].Symbol,
                stockPrice.CurrentPrice,
                stockDto.NumberOfStocks
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("sell-stock")]
    [Authorize]
    public async Task<IActionResult> SellStock([FromQuery] BuyStockDto stockDto)
    {
        throw new NotImplementedException();
    }

    [HttpGet("owned-stocks")]
    [Authorize]
    public async Task<IActionResult> GetOwnedStocks()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var stocks = await _stockService.GetAllStocks(bankAccountDetails.AccountNumber);

            var stocksResponse
                = stocks.Select(s => new
                {
                    StockName = s.StockName,
                    StockSymbol = s.StockSymbol,
                    StockId = s.StockId,
                    DateOfPurchase = s.DateOfPurchase.ToString("g"),
                    StockPrice = s.StockPrice,
                    NumberOfStocks = s.NumberOfStocks,
                    TotalAmountSpent = s.NumberOfStocks * s.StockPrice,
                    Currency = Enum.GetName(typeof(EnumCurrency),s.Currency),
                }).ToList();

            if (!stocksResponse.Any())
            {
                return Ok(new { Message = "No stocks found." });
            }

            return Ok(new
            {
                stocksResponse
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("stock-live-price")]
    [Authorize]
    public async Task<IActionResult> GetStockLivePrice([FromQuery] CurrencySymbolDto currencySymbolDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await _stockService.GetStockLivePrice(currencySymbolDto.Symbol));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("stock-historical")]
    [Authorize]
    public async Task<IActionResult> GetStockPrices([FromQuery] StockPricesDto stockPricesDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var stockPrices = await _stockService.GetStockPricesAsync(stockPricesDto);

        if (stockPrices.IsNullOrEmpty())
        {
            return BadRequest("Something went wrong.");
        }

        return Ok(stockPrices);
    }

    [HttpGet("top-gainers")]
    [Authorize]
    public async Task<IActionResult> GetTopGainers()
    {
        var topGainersResponse = await _stockService.GetTopGainers();

        if (topGainersResponse == null)
        {
            return BadRequest("Something went wrong.");
        }

        return Ok(topGainersResponse);
    }

    [HttpGet("top-losers")]
    [Authorize]
    public async Task<IActionResult> GetTopLosers()
    {
        var topLosersResponse = await _stockService.GetTopLosers();

        if (topLosersResponse == null)
        {
            return BadRequest("Something went wrong.");
        }

        return Ok(topLosersResponse);
    }

    [HttpGet("most-actively")]
    [Authorize]
    public async Task<IActionResult> GetMostActively()
    {
        var mostActivelyTradedResponse = await _stockService.GetMostActivelyStocks();

        if (mostActivelyTradedResponse == null)
        {
            return BadRequest("Something went wrong.");
        }

        return Ok(mostActivelyTradedResponse);
    }
}
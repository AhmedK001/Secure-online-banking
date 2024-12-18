using Application.DTOs;
using Application.DTOs.ExternalModels;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public StocksController(IOperationService operationService, UserManager<User> userManager,
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

            return Ok(await _stockService.GetStockLivePrice(currencySymbolDto.Symbol));
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
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Web.Controllers;

[ApiController]
[Route("api/stocks")]
public class StocksController : ControllerBase
{
    private readonly IStockService _stockService;

    public StocksController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet("stock")]
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
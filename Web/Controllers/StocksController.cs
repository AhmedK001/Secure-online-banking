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

    [HttpGet("get-stock")]
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

    [HttpGet("get-top-gainers")]
    [Authorize]
    public async Task<IActionResult> GetTopGainers()
    {
        var stockPrices = await _stockService.GetTopGainersAndLosersAsync();

        if (stockPrices.IsNullOrEmpty())
        {
            return BadRequest("Something went wrong.");
        }

        return Ok(stockPrices);
    }
}
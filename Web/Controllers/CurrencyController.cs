using System.Text.Json;
using System.Text.Json.Nodes;
using Application.DTOs.ExternalModels.Currency;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Web.Controllers;

[ApiController]
[Route("api/currency")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet("get-exchange-rate")]
    [Authorize]
    public async Task<IActionResult> GetExchangeRate([FromQuery] string currentCurrency,[FromQuery] string aimedCurrency)
    {
        if (string.IsNullOrWhiteSpace(currentCurrency) || string.IsNullOrWhiteSpace(aimedCurrency))
        {
            return BadRequest("Invalid input. Please provide both currentCurrency and aimedCurrency.");
        }

        try
        {
            var exchangeRateResult = await _currencyService.GetExchangeRate(currentCurrency, aimedCurrency);
            if (exchangeRateResult == null)
            {
                return NotFound("Exchange rate not found.");
            }

            return Ok(exchangeRateResult);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An error occurred while fetching the exchange rate. Please try again later.");
        }
    }

    [HttpGet("get-historical-exchange-rate")]
    public async Task<IActionResult> GetHistoricalExchangeRate([FromQuery] string currentCurrency,[FromQuery] string aimedCurrency, [FromQuery] string timeSeries)
    {
        if (string.IsNullOrWhiteSpace(currentCurrency) || string.IsNullOrWhiteSpace(aimedCurrency) || timeSeries.IsNullOrEmpty())
        {
            return BadRequest("Invalid input. Please provide both currentCurrency and aimedCurrency.");
        }

        try
        {
            var historicalExchangeRateResult
                = await _currencyService.GetHistoricalExchangeRate(currentCurrency, aimedCurrency, timeSeries);

            if (historicalExchangeRateResult == null)
            {
                return NotFound("Historical exchange rate not found.");
            }

            return Ok(historicalExchangeRateResult);
        }

        catch (Exception e)
        {
            return StatusCode(500, "An error occurred while fetching the exchange rate. Please try again later.");
        }
    }

}
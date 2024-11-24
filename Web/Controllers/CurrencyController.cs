using System.Text.Json;
using System.Text.Json.Nodes;
using Application.DTOs.ExternalModels.Currency;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Application.Interfaces;
using Application.Services;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Web.Controllers;

[ApiController]
[Route("api/currencies")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet("exchange-rate")]
    [Authorize]
    public async Task<IActionResult> GetExchangeRate([FromQuery] string currentCurrency,[FromQuery] string aimedCurrency)
    {
        if (string.IsNullOrWhiteSpace(currentCurrency) || string.IsNullOrWhiteSpace(aimedCurrency))
        {
            return BadRequest("Invalid input. Please provide both currentCurrency and aimedCurrency.");
        }

        try
        {
            EnumCurrency.TryParse(currentCurrency,out EnumCurrency fromCurrencye);
            EnumCurrency.TryParse(aimedCurrency,out EnumCurrency toCurrency);


            var exchangeRateResult = await _currencyService.GetExchangeRate(fromCurrencye, toCurrency);
            if (exchangeRateResult.data.IsNullOrEmpty())
            {
                return NotFound("Exchange rate not found.");
            }

            return Ok(exchangeRateResult.data);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"An error occurred while fetching the exchange rate. Please try again later.\n{e}");
        }
    }

    [HttpGet("historical-exchange-rate")]
    public async Task<IActionResult> GetHistoricalExchangeRate([FromQuery] string currentCurrency,[FromQuery] string aimedCurrency, [FromQuery] string timeSeries)
    {
        if (string.IsNullOrWhiteSpace(currentCurrency) || string.IsNullOrWhiteSpace(aimedCurrency) || timeSeries.IsNullOrEmpty())
        {
            return BadRequest("Invalid input. Please provide both currentCurrency and aimedCurrency.");
        }

        try
        {
            EnumCurrency.TryParse(currentCurrency,out EnumCurrency fromCurrencye);
            EnumCurrency.TryParse(aimedCurrency,out EnumCurrency toCurrency);


            var historicalExchangeRateResult
                = await _currencyService.GetHistoricalExchangeRate(fromCurrencye, toCurrency, timeSeries);

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
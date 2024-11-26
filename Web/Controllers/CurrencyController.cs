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

    public async Task<IActionResult> GetExchangeRate([FromQuery] string currentCurrency,
        [FromQuery] string aimedCurrency)
    {
        // Make an independent method for Direct exchange rate, Not related to card or bank account
        // make a DTO with validations for this method
        // make a DTO with validations for this method
        if (string.IsNullOrWhiteSpace(currentCurrency) || string.IsNullOrWhiteSpace(aimedCurrency))
        {
            return BadRequest("Invalid input. Please provide both currentCurrency and aimedCurrency.");
        }

        try
        {
            EnumCurrency.TryParse(currentCurrency, out EnumCurrency fromCurrencye);
            EnumCurrency.TryParse(aimedCurrency, out EnumCurrency toCurrency);


            // If current currency is AED or SAR and target is AED or SAR, so it can be converted directly
            bool letThemPass = (fromCurrencye == EnumCurrency.SAR || fromCurrencye == EnumCurrency.AED) &&
                               (toCurrency == EnumCurrency.SAR || toCurrency == EnumCurrency.AED);

            // Any else must contain USD or EUR as current account currency or USD or EUR as a target
            if (letThemPass == false && !(toCurrency == EnumCurrency.USD || toCurrency == EnumCurrency.EUR ||
                                          fromCurrencye == EnumCurrency.USD ||
                                          fromCurrencye == EnumCurrency.EUR))
                // Validate if given are real currencies or not before executing
            {
                return BadRequest(new
                {
                    ErrorMessage
                        = $"Exchange rate not directly available between {fromCurrencye} and {toCurrency}.",
                    Solution
                        = $"Try between {fromCurrencye} and USD or EUR or Between {toCurrency} and USD or EUR."
                });

            }

            var exchangeRateResult = await _currencyService.GetExchangeRate(fromCurrencye, toCurrency);
            if (exchangeRateResult.data.IsNullOrEmpty())
            {
                return NotFound("Exchange rate not found.");
            }

            return Ok(exchangeRateResult.data);
        }
        catch (Exception e)
        {
            return StatusCode(500,
                $"An error occurred while fetching the exchange rate. Please try again later.\n{e}");
        }
    }

    [HttpGet("historical-exchange-rate")]
    public async Task<IActionResult> GetHistoricalExchangeRate([FromQuery] string baseCurrency,
        [FromQuery] string targetCurrency, [FromQuery] string timeSeries)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(targetCurrency) ||
            timeSeries.IsNullOrEmpty())
        {
            return BadRequest("Invalid input. Please provide both currentCurrency and aimedCurrency.");
        }

        try
        {
            EnumCurrency.TryParse(baseCurrency, out EnumCurrency fromCurrencye);
            EnumCurrency.TryParse(targetCurrency, out EnumCurrency toCurrency);


            // If current currency is AED or SAR and target is AED or SAR, so it can be converted directly
            bool letThemPass = (fromCurrencye == EnumCurrency.SAR || fromCurrencye == EnumCurrency.AED) &&
                               (toCurrency == EnumCurrency.SAR || toCurrency == EnumCurrency.AED);

            // Any else must contain USD or EUR as current account currency or USD or EUR as a target
            if (letThemPass == false && !(toCurrency == EnumCurrency.USD || toCurrency == EnumCurrency.EUR ||
                                          fromCurrencye == EnumCurrency.USD ||
                                          fromCurrencye == EnumCurrency.EUR))
            {
                return BadRequest(new
                {
                    ErrorMessage
                        = $"Exchange rate not directly available between {fromCurrencye} and {toCurrency}.",
                    Solution
                        = $"Search for {fromCurrencye} to USD or EUR or From USD or EUR to {toCurrency}."
                });

            }

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
            return StatusCode(500,
                "An error occurred while fetching the exchange rate. Please try again later.");
        }
    }
}
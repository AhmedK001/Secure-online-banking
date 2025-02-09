﻿using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Application.DTOs;
using Application.DTOs.ExternalModels.Currency;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Application.DTOs.ResponseDto;
using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Web.Controllers;

[ApiController]
[Route("api/currencies")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly IClaimsService _claimsService;
    private readonly ICardsService _cardsService;
    private readonly IBankAccountService _bankAccountService;
    private readonly IValidate _validate;
    private readonly IEmailService _emailService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly IOperationService _operationService;
    private readonly IMemoryCache _memoryCache;

    public CurrencyController(IMemoryCache memoryCache,IOperationService operationService,UserManager<User> userManager,IEmailService emailService, IEmailBodyBuilder emailBodyBuilder, IConfiguration configuration,IValidate validate, ICurrencyService currencyService, IClaimsService claimsService,
        ICardsService cardsService, IBankAccountService bankAccountService)
    {
        _currencyService = currencyService;
        _claimsService = claimsService;
        _cardsService = cardsService;
        _bankAccountService = bankAccountService;
        _validate = validate;
        _configuration = configuration;
        _emailService = emailService;
        _emailBodyBuilder = emailBodyBuilder;
        _userManager = userManager;
        _operationService = operationService;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Retrieve the current exchange rate between two currencies.
    /// </summary>
    /// <param name="baseCurrency"></param>
    /// <param name="targetCurrency"></param>
    /// <returns></returns>
    [HttpGet("exchange/rate")]
    [Authorize]
    public async Task<IActionResult> GetExchangeRate([FromQuery] string baseCurrency,
        [FromQuery] string targetCurrency)
    {
        // Make an independent method for Direct exchange rate, Not related to card or bank account
        // make a DTO with validations for this method
        // make a DTO with validations for this method
        if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(targetCurrency) || baseCurrency == targetCurrency)
        {
            return BadRequest(new{ErrorMessage = "Invalid operation. Please provide both currentCurrency and aimedCurrency."});
        }

        try
        {

            JsonObject data;
            string key = $"{baseCurrency}{targetCurrency}";
            if (_memoryCache.TryGetValue(key, out JsonObject? jsonObject))
            {
                data = jsonObject;
            }
            else
            {
                var exchangeRateResult = await _currencyService.GetExchangeRate(baseCurrency, targetCurrency);
                if (exchangeRateResult.data.IsNullOrEmpty())
                {
                    return NotFound("Exchange rate not found.");
                }

                data = exchangeRateResult.data;
                _memoryCache.Set(key, exchangeRateResult.data,TimeSpan.FromMinutes(5));
            }

            return Ok(data);
        }
        catch (Exception e)
        {
            return BadRequest(new { ErrorMessage = $"{e.InnerException.Message}",Details = "In order if valid currencies, Means direct exchange between them is invalid" });
        }
    }

    /// <summary>
    /// Exchange balance between two of your linked cards.
    /// </summary>
    /// <param name="dtoCardToCard"></param>
    /// <returns></returns>
    [HttpPost("exchange/cards")]
    [Authorize]
    public async Task<IActionResult> CardToCardExchangeMoney(ExchangeMoneyDtoCardToCard dtoCardToCard)
    {
        try
        {
            if (dtoCardToCard.BaseCardId == dtoCardToCard.TargetCardId)
                return BadRequest(new { ErrorMessage = "Base and Target cards must be different" });

            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            var baseCard = await _cardsService.GetCardDetails(bankAccount.AccountNumber, dtoCardToCard.BaseCardId);

            if (dtoCardToCard.Amount < 5)
                return BadRequest(new { Message = $"The minimum amount must be 5 {bankAccount.Currency}" });

            if (dtoCardToCard.Amount > baseCard.Balance)
                return BadRequest(new
                {
                    ErrorMessage = $"Balance is not enough, Your balance is: {baseCard.Balance}{baseCard.Currency}"
                });
            var targetCard = await _cardsService.GetCardDetails(bankAccount.AccountNumber, dtoCardToCard.TargetCardId);
            var result = await _cardsService.CardToCardExchange(dtoCardToCard, baseCard, targetCard);

            if (!result.Item1) return BadRequest(new { ErrorMessage = result.Item2 });


            var baseCardAfterTransaction
                = await _cardsService.GetCardDetails(bankAccount.AccountNumber, dtoCardToCard.BaseCardId);
            var targetCardAfterTransaction
                = await _cardsService.GetCardDetails(bankAccount.AccountNumber, dtoCardToCard.TargetCardId);

            // save operation
            var operation = await _operationService.BuildExchangeOperation(bankAccount, baseCard, targetCard,
                dtoCardToCard.Amount, EnumOperationType.ExchangeCardToCard);
            await _operationService.AddOperation(true, operation);

            var emailBody = _emailBodyBuilder.CardToCardExchangeHtmlResponse(
                "Your card-to-card transaction done successfully.",
                baseCardAfterTransaction,
                targetCardAfterTransaction,
                dtoCardToCard.Amount
            );

            await _emailService.SendEmailAsync(user, "Card-to-Card exchange completed successfully", emailBody);

            return Ok(new SuccessResponseDto
            {
                Status = "Success",
                Message = "Card-to-Card exchange completed successfully.",
                Data = new
                {
                    BaseCard = new
                    {
                        CardId = baseCardAfterTransaction.CardId,
                        CardType = Enum.GetName(typeof(EnumCardType), baseCardAfterTransaction.CardType),
                        Currency = Enum.GetName(typeof(EnumCurrency), baseCardAfterTransaction.Currency),
                        Balance = baseCardAfterTransaction.Balance
                    },
                    TargetCard = new
                    {
                        CardId = targetCardAfterTransaction.CardId,
                        CardType = Enum.GetName(typeof(EnumCardType), targetCardAfterTransaction.CardType),
                        Currency = Enum.GetName(typeof(EnumCurrency), targetCardAfterTransaction.Currency),
                        Balance = targetCardAfterTransaction.Balance
                    },
                    ExchangeDetails = new
                    {
                        AmountExchanged = dtoCardToCard.Amount,
                    }
                }
            });
        }
        catch (Exception e)
        {
            return BadRequest(new { ErrorMessage = "Cannot exchange money at this moment." });
        }
    }

    /// <summary>
    /// Exchange balance from your bank account to a linked cards.
    /// </summary>
    /// <param name="exchangeDto"></param>
    /// <returns></returns>
    [HttpPost("exchange/banks-to-cards")]
    [Authorize]
    public async Task<IActionResult> BankToCardExchangeMoney(ExchangeMoneyDtoBankAndCard exchangeDto)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            var card = await _cardsService.GetCardDetails(bankAccount.AccountNumber, exchangeDto.CardId);

            if (bankAccount.Currency == card.Currency)
                return BadRequest(new { ErrorMessage = "Bank account and Card currencies must be different." });
            if (exchangeDto.Amount < 5)
                return BadRequest(new { Message = $"The minimum amount must be 5 {bankAccount.Currency}" });
            if (exchangeDto.Amount > bankAccount.Balance)
                return BadRequest(new
                {
                    ErrorMessage = $"Balance is not enough, Your balance is: {bankAccount.Balance}{bankAccount.Currency}"
                });
            var result = await _bankAccountService.BankWithCardExchange(true,exchangeDto,card,bankAccount);

            if (!result.Item1) return BadRequest(new { ErrorMessage = result.Item2 });

            // save operation
            var operation = await _operationService.BuildExchangeOperation(bankAccount, null, card,
                exchangeDto.Amount, EnumOperationType.ExchangeToCard);
            await _operationService.AddOperation(true, operation);

            var bankAccountAfterTransaction
                = await _bankAccountService.GetDetailsByAccountNumber(bankAccount.AccountNumber);
            var cardAfterTransaction
                = await _cardsService.GetCardDetails(bankAccount.AccountNumber, exchangeDto.CardId);

            var emailBody = _emailBodyBuilder.BankToCardExchangeHtmlResponse(
                "Your bank-to-card exchange operation done successfully.",
                bankAccountAfterTransaction,
                cardAfterTransaction,
                exchangeDto.Amount
            );

            await _emailService.SendEmailAsync(user, "Bank-to-card exchange completed successfully", emailBody);
            return Ok(new SuccessResponseDto() { Status = "Success", Message = $"Exchanged successfully." });
        }
        catch (Exception e)
        {
            return BadRequest(new { ErrorMessage = "Cannot exchange money at this moment." });
        }
    }

    /// <summary>
    /// Exchange balance from any of your cards to your bank account.
    /// </summary>
    /// <param name="exchangeDto"></param>
    /// <returns></returns>
    [HttpPost("exchange/cards-to-banks")]
    [Authorize]
    public async Task<IActionResult> CardToBankExchangeMoney(ExchangeMoneyDtoBankAndCard exchangeDto)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            var card = await _cardsService.GetCardDetails(bankAccount.AccountNumber, exchangeDto.CardId);

            if (bankAccount.Currency == card.Currency)
                return BadRequest(new { ErrorMessage = "Bank account and Card currencies must be different." });
            if (exchangeDto.Amount < 5)
                return BadRequest(new { Message = $"The minimum amount must be 5 {card.Currency}" });
            if (exchangeDto.Amount > card.Balance)
                return BadRequest(new
                {
                    ErrorMessage = $"Balance is not enough, Your balance is: {card.Balance}{card.Currency}"
                });
            var result = await _bankAccountService.BankWithCardExchange(false,exchangeDto,card,bankAccount);

            if (!result.Item1) return BadRequest(new { ErrorMessage = result.Item2 });

            // save operation
            var operation = await _operationService.BuildExchangeOperation(bankAccount, card, null,
                exchangeDto.Amount, EnumOperationType.ExchangeToAccount);
            await _operationService.AddOperation(true, operation);

            var bankAccountAfterTransaction
                = await _bankAccountService.GetDetailsByAccountNumber(bankAccount.AccountNumber);
            var cardAfterTransaction
                = await _cardsService.GetCardDetails(bankAccount.AccountNumber, exchangeDto.CardId);

            var emailBody = _emailBodyBuilder.CardToBankExchangeHtmlResponse(
                "Your card-to-bank exchange operation done successfully.",
                bankAccountAfterTransaction,
                cardAfterTransaction,
                exchangeDto.Amount
            );

            await _emailService.SendEmailAsync(user, "Card-to-bank exchange completed successfully", emailBody);

            return Ok(new SuccessResponseDto() { Status = "Success", Message = $"Exchanged successfully." });
        }
        catch (Exception e)
        {
            return BadRequest(new { ErrorMessage = "Cannot exchange money at this moment." });
        }
    }

    /// <summary>
    /// Retrieve historical exchange rates for a specified currency.
    /// </summary>
    /// <param name="baseCurrency"></param>
    /// <param name="targetCurrency"></param>
    /// <param name="timeSeries"></param>
    /// <returns></returns>
    [HttpGet("exchange/historical")]
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

            JsonObject historicalExchangeRateResult;
            string key = $"{fromCurrencye}{toCurrency}";
            if (_memoryCache.TryGetValue(key, out JsonObject? data))
            {
                historicalExchangeRateResult = data;
            }
            else
            {
                // If current currency is AED or SAR and target is AED or SAR, so it can be converted directly
                bool letThemPass = (fromCurrencye == EnumCurrency.SAR || fromCurrencye == EnumCurrency.AED) &&
                                   (toCurrency == EnumCurrency.SAR || toCurrency == EnumCurrency.AED);

                // Any else must contain USD or EUR as current account currency or USD or EUR as a target
                if (letThemPass == false && !(toCurrency == EnumCurrency.USD || toCurrency == EnumCurrency.EUR ||
                                              fromCurrencye == EnumCurrency.USD || fromCurrencye == EnumCurrency.EUR))
                {
                    return BadRequest(new
                    {
                        ErrorMessage
                            = $"Exchange rate not directly available between {fromCurrencye} and {toCurrency}.",
                        Solution = $"Search for {fromCurrencye} to USD or EUR or From USD or EUR to {toCurrency}."
                    });
                }

                historicalExchangeRateResult
                    = await _currencyService.GetHistoricalExchangeRate(fromCurrencye, toCurrency, timeSeries);
                _memoryCache.Set(key, historicalExchangeRateResult, TimeSpan.FromMinutes(int.Parse(timeSeries)));
                if (historicalExchangeRateResult == null)
                {
                    return NotFound("Historical exchange rate not found.");
                }
            }

            return Ok(historicalExchangeRateResult);
        }

        catch (Exception e)
        {
            return StatusCode(500, "An error occurred while fetching the exchange rate. Please try again later.");
        }
    }
}
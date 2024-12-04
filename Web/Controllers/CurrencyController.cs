using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Application.DTOs;
using Application.DTOs.ExternalModels.Currency;
using Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Application.DTOs.ResponseDto;
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
    private readonly IClaimsService _claimsService;
    private readonly ICardsService _cardsService;
    private readonly IBankAccountService _bankAccountService;
    private readonly IValidate _validate;
    private readonly IEmailService _emailService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;
    private readonly IConfiguration _configuration;

    public CurrencyController(IEmailService emailService, IEmailBodyBuilder emailBodyBuilder, IConfiguration configuration,IValidate validate, ICurrencyService currencyService, IClaimsService claimsService,
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
            // EnumCurrency.TryParse(currentCurrency, out EnumCurrency fromCurrencye);
            // EnumCurrency.TryParse(aimedCurrency, out EnumCurrency toCurrency);
            //
            //
            // // If current currency is AED or SAR and target is AED or SAR, so it can be converted directly
            // bool letThemPass = (fromCurrencye == EnumCurrency.SAR || fromCurrencye == EnumCurrency.AED) &&
            //                    (toCurrency == EnumCurrency.SAR || toCurrency == EnumCurrency.AED);
            //
            // // Any else must contain USD or EUR as current account currency or USD or EUR as a target
            // if (letThemPass == false && !(toCurrency == EnumCurrency.USD || toCurrency == EnumCurrency.EUR ||
            //                               fromCurrencye == EnumCurrency.USD ||
            //                               fromCurrencye == EnumCurrency.EUR))
            //     // Validate if given are real currencies or not before executing
            // {
            //     return BadRequest(new
            //     {
            //         ErrorMessage
            //             = $"Exchange rate not directly available between {fromCurrencye} and {toCurrency}.",
            //         Solution
            //             = $"Try between {fromCurrencye} and USD or EUR or Between {toCurrency} and USD or EUR."
            //     });
            //
            // }
            var exchangeRateResult = await _currencyService.GetExchangeRate(currentCurrency, aimedCurrency);
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

    [HttpPost("card-to-card-exchange")]
    [Authorize]
    public async Task<IActionResult> CardToCardExchangeMoney(ExchangeMoneyDtoCardToCard dtoCardToCard)
    {
        try
        {
            if (dtoCardToCard.BaseCardId == dtoCardToCard.TargetCardId)
                return BadRequest(new { ErrorMessage = "Base and Target cards must be different" });

            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
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

            var emailBody = _emailBodyBuilder.CardToCardExchange(
                "Card-to-Card exchange completed successfully.",
                baseCardAfterTransaction,
                targetCardAfterTransaction,
                dtoCardToCard.Amount
            );

            var email = _configuration["Email"];

            await _emailService.SendEmailAsync(email, "Card-to-Card Exchange Completed", emailBody);

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
            return BadRequest(e.Message);
        }
    }

    [HttpPost("bank-to-card-exchange")]
    [Authorize]
    public async Task<IActionResult> BankToCardExchangeMoney(ExchangeMoneyDtoBankAndCard exchangeDto)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
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


            var bankAccountAfterTransaction
                = await _bankAccountService.GetDetailsByAccountNumber(bankAccount.AccountNumber);
            var cardAfterTransaction
                = await _cardsService.GetCardDetails(bankAccount.AccountNumber, exchangeDto.CardId);

            var email = _configuration["Email"];

            var emailBody = _emailBodyBuilder.BankToCardExchange(
                "Bank-to-Card exchange completed successfully.",
                bankAccountAfterTransaction,
                cardAfterTransaction,
                exchangeDto.Amount
            );

            await _emailService.SendEmailAsync(email, "Bank-to-Card Exchange Completed", emailBody);
            return Ok(new SuccessResponseDto() { Status = "Success", Message = $"Exchanged successfully." });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("card-to-bank-exchange")]
    [Authorize]
    public async Task<IActionResult> CardToBankExchangeMoney(ExchangeMoneyDtoBankAndCard exchangeDto)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
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

            var bankAccountAfterTransaction
                = await _bankAccountService.GetDetailsByAccountNumber(bankAccount.AccountNumber);
            var cardAfterTransaction
                = await _cardsService.GetCardDetails(bankAccount.AccountNumber, exchangeDto.CardId);

            var email = _configuration["Email"];

            var emailBody = _emailBodyBuilder.CardToBankExchange(
                "Card-to-Bank exchange completed successfully.",
                bankAccountAfterTransaction,
                cardAfterTransaction,
                exchangeDto.Amount
            );

            await _emailService.SendEmailAsync(email, "Card-to-Bank Exchange Completed", emailBody);

            return Ok(new SuccessResponseDto() { Status = "Success", Message = $"Exchanged successfully." });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
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
                                          fromCurrencye == EnumCurrency.USD || fromCurrencye == EnumCurrency.EUR))
            {
                return BadRequest(new
                {
                    ErrorMessage
                        = $"Exchange rate not directly available between {fromCurrencye} and {toCurrency}.",
                    Solution = $"Search for {fromCurrencye} to USD or EUR or From USD or EUR to {toCurrency}."
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
            return StatusCode(500, "An error occurred while fetching the exchange rate. Please try again later.");
        }
    }
}
using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Web.Controllers;

[ApiController]
[Route("api/cards")]
public class CardsController : ControllerBase
{
    private readonly IClaimsService _claimsService;
    private readonly IBankAccountService _bankAccountService;
    private readonly ICardsService _cardsService;

    public CardsController(IClaimsService claimsService, IBankAccountService bankAccountService,
        ICardsService cardsService)
    {
        _claimsService = claimsService;
        _bankAccountService = bankAccountService;
        _cardsService = cardsService;
    }

    [HttpPost("card")]
    [Authorize]
    public async Task<IActionResult> CreateBankCard([FromQuery] string cardType)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var createCardResult = await _cardsService.CreateCardAsync(bankAccountDetails.AccountNumber,
                cardType, bankAccountDetails);

            var createdCardDetails
                = await _cardsService.GetCardDetails(bankAccountDetails.AccountNumber,
                    createCardResult.Item2);

            CardResponseDto cardResponseDto = new CardResponseDto()
            {
                Balance = createdCardDetails.Balance,
                ExpiryDate = createdCardDetails.ExpiryDate,
                IsActivated = createdCardDetails.IsActivated,
                OpenedForInternalOperations = createdCardDetails.OpenedForInternalOperations,
                OpenedForOnlinePurchase = createdCardDetails.OpenedForOnlinePurchase,
                CardType = createdCardDetails.CardType,
                CardId = createdCardDetails.CardId,
                Cvv = createdCardDetails.Cvv
            };

            return Ok(new
            {
                Message = "You card has been created successfully.", CreatedCardDetails = cardResponseDto
            });
        }
        catch (Exception e)
        {
            return StatusCode(500,
                new
                {
                    ErrorMessage = e.Message
                });
        }
    }

    [HttpPut("change-currency")]
    [Authorize]
    public async Task<IActionResult> ChangeCurrency([FromBody] ChangeCurrencyCardDto currencyCardDto)
    {
        try
        {
            // User cannot exchange directly from, or to EGP, TRY, SAR, AED until convert to USD or EUR, then convert to the target

            // IF ACCOUNT GOT MONEY SO ASK FOR EXCHANGE BEFORE CHANGING CURRENCY
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var aimedCard = await _cardsService.GetCardDetails(bankAccountDetails.AccountNumber, currencyCardDto.CardId);

            // make sure if aimed card is its own card.
            if (aimedCard.BankAccount.AccountNumber != bankAccountDetails.AccountNumber)
            {
                return BadRequest("You do not have any cards with in this ID number.");
            }

            if (!Enum.TryParse(currencyCardDto.AimedCurrencySymbol, out EnumCurrency currencySymbol))
            {
                var availableSymbols = string.Join(", ", Enum.GetNames(typeof(EnumCurrency)));
                return BadRequest(
                    $"Currency symbol not found. Available currencies to use are: {availableSymbols}");
            }

            if (aimedCard.Currency == currencySymbol)
            {
                return BadRequest("Your account uses the same currency already.");
            }

            // If current currency is AED or SAR and target is AED or SAR, so it can be converted directly
            bool letThemPass = ((Enum.GetName(typeof(EnumCurrency), aimedCard.Currency) == "SAR") ||
                                Enum.GetName(typeof(EnumCurrency), aimedCard.Currency) == "AED") &&
                               (currencySymbol == EnumCurrency.SAR || currencySymbol == EnumCurrency.AED);

            // Any else must contain USD or EUR as current account currency or USD or EUR as a target
            if (letThemPass == false &&
                !(aimedCard.Currency == EnumCurrency.USD ||
                  aimedCard.Currency == EnumCurrency.EUR || currencySymbol == EnumCurrency.USD ||
                  currencySymbol == EnumCurrency.EUR))
            {
                return BadRequest($"You must exchange to USD or AED then exchange to {currencySymbol.ToString()}");
            }

            await _cardsService.ChangeCurrencyAsync(currencySymbol, currencyCardDto.CardId,
                bankAccountDetails.AccountNumber);

            var cardAfterCurrencyChanged
                = await _cardsService.GetCardDetails(bankAccountDetails.AccountNumber, currencyCardDto.CardId);

            return Ok(new
            {
                Card = new
                {
                    cardAfterCurrencyChanged.CardId,
                    CardType = Enum.GetName(typeof(EnumCardType),cardAfterCurrencyChanged.CardType),
                    cardAfterCurrencyChanged.IsActivated,
                    Currency = Enum.GetName(typeof(EnumCurrency), cardAfterCurrencyChanged.Currency),
                    cardAfterCurrencyChanged.Balance
                }
            });
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }

    [HttpGet("all-cards")]
    [Authorize]
    public async Task<IActionResult> GetAllCards()
    {
        var userId = await _claimsService.GetUserIdAsync(User);
        if (!await _bankAccountService.IsUserHasBankAccount(Guid.Parse(userId)))
        {
            return BadRequest("You must create your bank account in order to use this service");
        }

        try
        {
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var cards = await _cardsService.GetAllCards(bankAccount.AccountNumber);

            var cardList = cards.Select(card => new
            {
                CardId = card.CardId,
                CVV = card.Cvv,
                ExpiryDate = card.ExpiryDate.ToString("dd-MM-yyyy"),
                CardType = Enum.GetName(typeof(EnumCardType), card.CardType),
                Currency = Enum.GetName(typeof(EnumCurrency), card.Currency),
                Balance = card.Balance,
                IsActivated = card.IsActivated,
                OpenedForOnlinePurchase = card.OpenedForOnlinePurchase,
                OpenedForInternalOperations = card.OpenedForInternalOperations,
            }).ToList();

            if (cardList.IsNullOrEmpty())
            {
                return NotFound("You do not have any cards yet.");
            }

            return Ok(new { Message = "These are your current Cards.", Cards = cardList });
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }

    [HttpDelete("card")]
    [Authorize]
    public async Task<IActionResult> DeleteCard([FromQuery] int cardId)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var aimedCard = await _cardsService.GetCardDetails(bankAccountDetails.AccountNumber, cardId);

            // make sure if aimed card is its own card.
            if (aimedCard.BankAccount.AccountNumber != bankAccountDetails.AccountNumber)
            {
                return BadRequest("You do not have any cards with in this ID number.");
            }

            await _cardsService.DeleteCard(aimedCard.CardId);
            return Ok("You card does not exist any more.");
        }
        catch (Exception e)
        {
            return BadRequest("You do not have any cards with in this ID number.");
        }
    }
}
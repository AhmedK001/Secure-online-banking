using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("cards")]
public class CardsController : ControllerBase
{
    private readonly IClaimsService _claimsService;
    private readonly IBankAccountService _bankAccountService;
    private readonly ICardsService _cardsService;

    public CardsController(IClaimsService claimsService,
        IBankAccountService bankAccountService, ICardsService cardsService)
    {
        _claimsService = claimsService;
        _bankAccountService = bankAccountService;
        _cardsService = cardsService;
    }

    [HttpGet("card")]
    [Authorize]
    public async Task<IActionResult> CreateBankCard([FromQuery] string cardType)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var createCardResult = await _cardsService.CreateCardAsync(bankAccountDetails.AccountNumber,
                cardType,bankAccountDetails);

            var createdCardDetails = await _cardsService.GetCardDetails(bankAccountDetails.AccountNumber,createCardResult.Item2);

            CardResponseDto cardResponseDto = new CardResponseDto()
            {
                Balance = createdCardDetails.Balance,
                ExpiryDate = createdCardDetails.ExpiryDate,
                IsActivated = createdCardDetails.IsActivated,
                OpenedForInternalOperations
                    = createdCardDetails.OpenedForInternalOperations,
                OpenedForOnlinePurchase
                    = createdCardDetails.OpenedForOnlinePurchase,
                CardType = createdCardDetails.CardType,
                CardId = createdCardDetails.CardId,
                Cvv = createdCardDetails.Cvv
            };

            return Ok(new
            {
                Message = "You card has been created successfully.",
                CreatedCardDetails = cardResponseDto
            });
        }
        catch (Exception e)
        {
            return StatusCode(500,
                new
                {
                    Message = "Some error occurred",
                    ErrorType = e.GetType().Name,
                    ErrorMessage = e.Message,
                    StackTrace = e.StackTrace
                });
        }
    }
}
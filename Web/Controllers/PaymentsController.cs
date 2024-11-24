using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using BankAccount = Core.Entities.BankAccount;
using Card = Core.Entities.Card;

namespace Web.Controllers;

[Route("api/payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private static decimal _chargeAmount;
    private readonly IBankAccountService _bankAccountService;
    private readonly ICardsService _cardsService;
    private readonly IClaimsService _claimsService;

    public PaymentsController(IConfiguration configuration, IBankAccountService bankAccountService,
        IClaimsService claimsService, ICardsService cardsService)
    {
        _configuration = configuration;
        _bankAccountService = bankAccountService;
        _claimsService = claimsService;
        _cardsService = cardsService;
    }


    [HttpPost("charge")]
    [Authorize]
    public async Task<IActionResult> Charge([FromBody] ChargeRequestDto chargeRequestDto)
    {
        // get user claims
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
        {
            return Unauthorized("User ID not found.");
        }

        if (!await _bankAccountService.IsUserHasBankAccount(userId))
        {
            return NotFound("You must create a bank account to be able to use these services");
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(chargeRequestDto.Amount * 100), //
            Currency = "usd",
            PaymentMethod = chargeRequestDto.PaymentMethodId,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true, AllowRedirects = "never"
            },
            Confirm = false
        };

        var service = new PaymentIntentService();
        try
        {
            // Create a PaymentIntent
            var paymentIntent = await service.CreateAsync(options);

            // Return the PaymentIntentId
            _chargeAmount = chargeRequestDto.Amount;
            return Ok(new { PaymentIntentId = paymentIntent.Id, Status = paymentIntent.Status });
        }
        catch (StripeException e)
        {
            return BadRequest(new { Error = e.StripeError.Message });
        }
    }


    [HttpPost("confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmRequestDto confirmRequestDto)
    {
        var service = new PaymentIntentService();
        try
        {
            // Confirm the Payment
            var paymentIntent = await service.ConfirmAsync(confirmRequestDto.PaymentIntentId,
                new PaymentIntentConfirmOptions());

            // get user claims
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            {
                return Unauthorized("User ID not found.");
            }

            if (!await _bankAccountService.IsUserHasBankAccount(userId))
            {
                return NotFound("You must create a bank account to be able to use these services");
            }

            var bankAccountDetails = await _bankAccountService.GetDetailsById(userId);

            if (bankAccountDetails == null)
            {
                return Ok(
                    $"You account has been charged with {_chargeAmount}\nAdditionally something went wrong while getting you bank account details");
            }


            var chargeBankResult
                = await _bankAccountService.ChargeAccount(userId, _chargeAmount, bankAccountDetails);

            if (chargeBankResult == false)
            {
                return BadRequest("Something went wrong.");
            }

            _chargeAmount = 0;
            return Ok(new { paymentIntent.Status, BankAccountDetails = bankAccountDetails });
        }


        catch (StripeException e)
        {
            return BadRequest(new { Error = e.StripeError.Message });
        }
    }

    private long ConvertDollarsToCents(decimal dollars)
    {
        return (long)(dollars * 100);
    }

    [HttpPost("transfer-to-card")]
    [Authorize]
    public async Task<IActionResult> TransferToCard([FromBody] TransferToCardDto cardDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = await _claimsService.GetUserIdAsync(User);

        if (!await _bankAccountService.IsUserHasBankAccount(Guid.Parse(userId)))
        {
            return BadRequest("You must create Bank Account in order to use this service.");
        }

        var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
        var cardDetails = await _cardsService.GetAllCards(bankAccountDetails.AccountNumber);

        Card? aimedCard = cardDetails.Find(c => c.CardId == cardDto.CardId);

        if (aimedCard == null)
        {
            return NotFound("No cards found under this ID number.");
        }

        // Make sure bank account and card uses same currencies
        if (bankAccountDetails.Currency != aimedCard.Currency)
        {
            return BadRequest(new
            {
                ErrorMessage = $"Currencies of Card and Bank Account does not match.",
                Currencies = $"Card currency :{aimedCard.Currency}, Bank Account currency :{bankAccountDetails.Currency}",
                Solution = "Use change currency API to change any of current currencies."
            });
        }

        if (!aimedCard.OpenedForInternalOperations || !aimedCard.IsActivated)
        {
            return BadRequest("You card not activated for this operation.");
        }

        if (bankAccountDetails.Balance < cardDto.Amount)
        {
            return BadRequest(
                $"No enough balance. Your Bank Account balance is: {bankAccountDetails.Balance} {bankAccountDetails.Currency}");
        }

        try
        {
            await _bankAccountService.DeductAccountBalance(bankAccountDetails.AccountNumber, cardDto.Amount);
            if (await _cardsService.ChargeCardBalanceAsync(bankAccountDetails.AccountNumber, aimedCard.CardId,
                    cardDto.Amount))
            {
                var cardAfterBalanceAdded
                    = await _cardsService.GetCardDetails(bankAccountDetails.AccountNumber, aimedCard.CardId);
                var bankAccountAfterBalanceDeducted
                    = await _bankAccountService.GetDetailsById(Guid.Parse(userId));

                return Ok(new
                {
                    Message = "Your card has been charged successfully.",
                    Card = new
                    {
                        CardId = cardAfterBalanceAdded.CardId,
                        Balance = cardAfterBalanceAdded.Balance,
                        CardType = Enum.GetName(typeof(EnumCardType),cardAfterBalanceAdded.CardType)
                    },
                    BankAccount = new
                    {
                        AccountNumber = bankAccountDetails.AccountNumber,
                        Balance = bankAccountAfterBalanceDeducted.Balance
                    }
                });
            }

            return BadRequest("Something went wrong");
        }
        catch (Exception e)
        {
            return BadRequest(new { e.StackTrace, e });
        }
    }
}
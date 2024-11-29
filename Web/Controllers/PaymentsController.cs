using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;
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
    private readonly IOperationService _operationService;
    private readonly IPaymentsService _paymentsService;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentsController(IConfiguration configuration, IBankAccountService bankAccountService,
        IClaimsService claimsService, ICardsService cardsService, IOperationService operationService,
        IPaymentsService paymentsService, IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _bankAccountService = bankAccountService;
        _claimsService = claimsService;
        _cardsService = cardsService;
        _operationService = operationService;
        _paymentsService = paymentsService;
        _unitOfWork = unitOfWork;
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

            var bankAccount = await _bankAccountService.GetDetailsById(userId);

            if (bankAccount == null)
            {
                return Ok(
                    $"You account has been charged with {_chargeAmount}\nAdditionally something went wrong while getting you bank account details");
            }


            var chargeBankResult = await _bankAccountService.ChargeAccount(userId, _chargeAmount, bankAccount);

            if (chargeBankResult == false)
            {
                return BadRequest("Something went wrong.");
            }

            // add currency for operation service, make default charge currency then validate.
            _chargeAmount = 0;
            return Ok(new
            {
                paymentIntent.Status,
                bankAccount.AccountNumber,
                Currency = Enum.GetName(typeof(EnumCurrency), bankAccount.Currency),
                bankAccount.Balance,
                bankAccount.UserId
            });
        }
        catch (StripeException e)
        {
            return BadRequest(new { Error = e.StripeError.Message });
        }
    }

    [HttpPost("transfer-to-card")]
    [Authorize]
    public async Task<IActionResult> TransferToCard([FromBody] TransferToCardDto cardDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var userId = await _claimsService.GetUserIdAsync(User);

            if (!await _bankAccountService.IsUserHasBankAccount(Guid.Parse(userId)))
            {
                return BadRequest("You must create Bank Account in order to use this service.");
            }

            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var cardDetails = await _cardsService.GetAllCards(bankAccountDetails.AccountNumber);

            Card aimedCard = cardDetails.Find(c => c.CardId == cardDto.CardId);

            // this method will throw exception if not valid transaction
            await _paymentsService.IsValidTransactionToCard(cardDto.Amount, userId, bankAccountDetails, aimedCard);


            await _bankAccountService.DeductAccountBalance(bankAccountDetails.AccountNumber, cardDto.Amount);
            await _cardsService.ChargeCardBalanceAsync(bankAccountDetails.AccountNumber, aimedCard.CardId,
                cardDto.Amount);

            var cardAfterBalanceAdded
                = await _cardsService.GetCardDetails(bankAccountDetails.AccountNumber, aimedCard.CardId);
            var bankAccountAfterBalanceDeducted = await _bankAccountService.GetDetailsById(Guid.Parse(userId));

            Operation operation = new Operation()
            {
                AccountNumber = bankAccountDetails.AccountNumber,
                AccountId = bankAccountDetails.NationalId,
                OperationId = await _operationService.GenerateUniqueRandomOperationIdAsync(),
                OperationType = EnumOperationType.TransactionToCard,
                Description = $"Transaction from your Bank account to your CARD," +
                              $" Amount transferred is: {cardDto.Amount:F2}{bankAccountDetails.Currency}",
                DateTime = DateTime.UtcNow,
                Currency = aimedCard.Currency,
                Amount = cardDto.Amount,
            };

            await _operationService.LogOperation(true, operation);
            await _unitOfWork.CommitTransactionAsync(); // commit changes if all succeeded

            return Ok(new
            {
                Message = "Your card has been charged successfully.",
                Card = new
                {
                    CardId = cardAfterBalanceAdded.CardId,
                    Currency = Enum.GetName(typeof(EnumCurrency), cardAfterBalanceAdded.Currency),
                    Balance = cardAfterBalanceAdded.Balance,
                    CardType = Enum.GetName(typeof(EnumCardType), cardAfterBalanceAdded.CardType)
                },
                BankAccount = new
                {
                    AccountNumber = bankAccountDetails.AccountNumber,
                    Currency = Enum.GetName(typeof(EnumCurrency), bankAccountAfterBalanceDeducted.Currency),
                    Balance = bankAccountAfterBalanceDeducted.Balance
                }
            });
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(e.Message);
        }
    }
}
using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.ResponseDto;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IEmailService _emailService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;
    private readonly UserManager<User> _userManager;

    public PaymentsController(UserManager<User> userManager,IEmailService emailService, IEmailBodyBuilder emailBodyBuilder,
        IConfiguration configuration, IBankAccountService bankAccountService, IClaimsService claimsService,
        ICardsService cardsService, IOperationService operationService, IPaymentsService paymentsService,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _bankAccountService = bankAccountService;
        _claimsService = claimsService;
        _cardsService = cardsService;
        _operationService = operationService;
        _paymentsService = paymentsService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _emailBodyBuilder = emailBodyBuilder;
        _userManager = userManager;
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

        if (!Enum.TryParse<EnumPaymentMethods>(chargeRequestDto.PaymentMethod, out var paymentMethod))
        {
            var names = string.Join(", ", Enum.GetNames<EnumPaymentMethods>());
            return BadRequest($"Accepted payment methods are: {names}.");
        }

        var bankAccount = await _bankAccountService.GetDetailsById(userId);
        var accountCurrency = Enum.GetName(typeof(EnumCurrency), bankAccount.Currency);
        if (accountCurrency != "EUR" && accountCurrency != "AED" && accountCurrency != "USD")
        {
            return BadRequest(new
            {
                error = "Invalid Currency",
                message = "The bank account currency must be one of the following: AED, USD, or EUR.",
                allowedCurrencies = new[] { "AED", "USD", "EUR" }
            });}

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(chargeRequestDto.Amount * 100),
            Currency = accountCurrency,
            PaymentMethod = paymentMethod.ToString(),
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
            return BadRequest(new { Error = e.Message });
        }
    }


    [HttpPut("confirm")]
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
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

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

            string emailContent = _emailBodyBuilder.ChargeAccountHtmlResponse("You account has been Charge Successfully!",
                bankAccount, _chargeAmount, paymentIntent.Status);

            await _emailService.SendEmailAsync(user.UserName, "You account has been Charge Successfully", emailContent);

            // add operation as logs
            await _operationService.ValidateAndSaveOperation(
                await _operationService.BuildChargeOperation(bankAccount, _chargeAmount));
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
    public async Task<IActionResult> TransferToCard([FromBody] InternalTransactionDto cardDto)
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
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
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

            // add operation as logs
            var operation = await _operationService.BuildTransferOperation(bankAccountDetails, cardDto.Amount,
                EnumOperationType.TransactionToCard);
            await _operationService.ValidateAndSaveOperation(operation);

            await _unitOfWork.CommitTransactionAsync(); // commit changes if all succeeded

            var email = _configuration["Email"];

            string emailContent = _emailBodyBuilder.TransferToCardHtmlResponse(
                "Your transaction to the card has been completed successfully.", bankAccountDetails, aimedCard,
                cardDto.Amount);

            await _emailService.SendEmailAsync(user.UserName, "Your transaction to the card has been completed successfully.",
                emailContent);


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

    [HttpPost("transfer-to-account")]
    [Authorize]
    public async Task<IActionResult> TransferToAccount(InternalTransactionDto transactionDto)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId)); // it check if null
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            var card = await _cardsService.GetCardDetails(bankAccount.AccountNumber,
                transactionDto.CardId); // it check if null

            if (card.Currency != bankAccount.Currency)
                return BadRequest(new
                {
                    ErrorMessage = "Bank account and Card currency does not match.",
                    Details = $"Bank account currency: {bankAccount.Currency}, Card currency: {card.Currency}"
                });

            if (card.Balance < transactionDto.Amount)
                return BadRequest(new
                {
                    ErrorMessage
                        = $"No enough balance, Your card balance is {card.Balance.ToString("F2")}{card.Currency}"
                });

            var result = await _cardsService.TransferToBankAccount(transactionDto, bankAccount, card);

            if (!result.Item1)
            {
                return BadRequest(new { ErrorMessage = result.Item2 });
            }

            var bankAccountAfterTransaction = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var cardAfterTransaction
                = await _cardsService.GetCardDetails(bankAccount.AccountNumber, transactionDto.CardId);

            var bankResponse = new
            {
                Balance = bankAccountAfterTransaction.Balance.ToString("F2"),
                Currency = Enum.GetName(typeof(EnumCurrency), bankAccountAfterTransaction.Currency),
            };
            var cardResponse = new
            {
                Balance = cardAfterTransaction.Balance.ToString("F2"),
                Currency = Enum.GetName(typeof(EnumCurrency), cardAfterTransaction.Currency),
            };

            string emailContent = _emailBodyBuilder.TransferToAccountHtmlResponse(
                "Your transaction to the bank account has been completed successfully.", bankAccount, card,
                transactionDto.Amount);

            await _emailService.SendEmailAsync(user.UserName,
                "Your transaction to the bank account has been completed successfully.", emailContent);

            var operation = await _operationService.BuildTransferOperation(bankAccount, transactionDto.Amount,
                EnumOperationType.TransactionToAccount);
            await _operationService.ValidateAndSaveOperation(operation);

            return Ok(new
            {
                Status = "Success",
                Message = $"Transferred successfully",
                CardResponse = cardResponse,
                BankAccount = bankResponse
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
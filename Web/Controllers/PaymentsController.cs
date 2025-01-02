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
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(ILogger<PaymentsController> logger, UserManager<User> userManager,
        IEmailService emailService, IEmailBodyBuilder emailBodyBuilder, IConfiguration configuration,
        IBankAccountService bankAccountService, IClaimsService claimsService, ICardsService cardsService,
        IOperationService operationService, IPaymentsService paymentsService, IUnitOfWork unitOfWork)
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
        _logger = logger;
    }


[HttpPost("charge")]
[Authorize]
public async Task<IActionResult> CreatePaymentIntentAsync(ChargeRequestDto chargeRequestDto)
{
    if (chargeRequestDto == null) return BadRequest("Charge request data is required.");

    var userId = await _claimsService.GetUserIdAsync(User);
    var user = await _userManager.FindByIdAsync(userId);
    if (user is null) return BadRequest("Invalid Request.");

    Guid guid = Guid.Parse(userId);
    var bankAccount = await _bankAccountService.GetDetailsById(guid);
    if (bankAccount == null) return BadRequest("Bank account not found.");

    if (!Enum.TryParse<EnumPaymentMethods>(chargeRequestDto.PaymentMethod, out var paymentMethod))
    {
        var names = string.Join(", ", Enum.GetNames<EnumPaymentMethods>());
        return BadRequest($"Accepted payment methods are: {names}.");
    }

    var isValidCurrency = _paymentsService.IsValidCurrencyTypeToCharge(bankAccount);
    if (!isValidCurrency.isValid)
        return BadRequest(new { isValidCurrency.ErrorMessage });

    long amountInCents;
    switch (bankAccount.Currency)
    {
        case EnumCurrency.USD:
            amountInCents = chargeRequestDto.Amount * 100;
            if (amountInCents < 50)
            {
                return BadRequest("Amount must be at least $0.50.");
            }
            break;

        case EnumCurrency.AED:
            amountInCents = chargeRequestDto.Amount;
            if (amountInCents < 20)
            {
                return BadRequest("Amount must be at least 0.20 AED.");
            }
            break;

        case EnumCurrency.EUR:
            amountInCents = chargeRequestDto.Amount * 100;
            if (amountInCents < 50)
            {
                return BadRequest("Amount must be at least €0.50.");
            }
            break;

        default:
            return BadRequest("Unsupported currency.");
    }

    // Stripe section
    var options = new PaymentIntentCreateOptions()
    {
        Amount = amountInCents,
        Currency = Enum.GetName(typeof(EnumCurrency), bankAccount.Currency),
        PaymentMethod = paymentMethod.ToString(),
        Confirm = false,
        AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions()
        {
            Enabled = true,
            AllowRedirects = "never"
        }
    };

    var paymentService = new PaymentIntentService();
    var paymentIntent = await paymentService.CreateAsync(options);

    return Ok(new
    {
        paymentIntent.Status,
        paymentIntent.Id,
        Amount = chargeRequestDto.Amount,
        paymentIntent.Currency,
        paymentIntent.CanceledAt,
        paymentIntent.AmountDetails
    });
}

[HttpPut("confirm")]
[Authorize]
public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmRequestDto confirmRequestDto)
{
    if (confirmRequestDto == null) return BadRequest("Confirmation data is required.");

    await _unitOfWork.BeginTransactionAsync();
    var service = new PaymentIntentService();

    try
    {
        var userId = await _claimsService.GetUserIdAsync(User);
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null) return BadRequest("Invalid Request.");

        Guid guid = Guid.Parse(userId);
        var bankAccount = await _bankAccountService.GetDetailsById(guid);

        if (bankAccount == null) return BadRequest("Bank account not found.");

        // Confirm the Payment Intent
        var paymentIntent = await service.ConfirmAsync(
            confirmRequestDto.PaymentIntentId,
            new PaymentIntentConfirmOptions()
        );

        // Calculate the amount to charge based on the currency
        decimal amountToCharge;
        switch (paymentIntent.Currency.ToLower())
        {
            case "usd":
            case "eur":
                amountToCharge = paymentIntent.Amount / 100m;
                break;
            case "aed":
                amountToCharge = paymentIntent.Amount;
                break;
            default:
                return BadRequest("Unsupported currency.");
        }

        // Charge the bank account
        var chargeBankResult = await _bankAccountService.ChargeAccount(guid, amountToCharge, bankAccount);

        if (!chargeBankResult)
        {
            return BadRequest("Something went wrong while charging the account.");
        }

        // Send email notification
        string emailContent = _emailBodyBuilder.ChargeAccountHtmlResponse(
            "Your account has been charged successfully!", bankAccount, amountToCharge, paymentIntent.Status
        );

        await _emailService.SendEmailAsync(user, "Your account has been charged successfully", emailContent);

        // Log the operation
        await _operationService.ValidateAndSaveOperation(
            await _operationService.BuildChargeOperation(bankAccount, paymentIntent.Amount)
        );

        await _unitOfWork.CommitTransactionAsync();
        return Ok(new { paymentIntent.Status, Message = "Charged successfully." });
    }
    catch (StripeException e)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return BadRequest(new { Error = e.StripeError.Message });
    }
    catch (Exception e)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return BadRequest(new { Error = e.Message });
    }
}


    [HttpPost("transfers/cards")]
    [Authorize]
    public async Task<IActionResult> TransferToCard([FromBody] InternalTransactionDto cardDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var cardDetails = await _cardsService.GetAllCards(bankAccountDetails.AccountNumber);

            Card aimedCard = cardDetails.Find(c => c.CardId == cardDto.CardId);

            var transactionResult
                = await _paymentsService.MakeTransactionToCard(user, bankAccountDetails, aimedCard, cardDto);

            if (!transactionResult.isSuccess) return BadRequest(new { transactionResult.ErrorMessage });

            return Ok(new { Message = "Your card has been charged successfully.", });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("transfers/accounts")]
    [Authorize]
    public async Task<IActionResult> TransferToAccount(InternalTransactionDto transactionDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return BadRequest("Invalid Request.");

            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));

            var card = await _cardsService.GetCardDetails(bankAccount.AccountNumber, transactionDto.CardId);

            var transactionResult
                = await _paymentsService.MakeTransactionToBank(user, bankAccount, card, transactionDto);

            if (!transactionResult.isSuccess) return BadRequest(new { transactionResult.ErrorMessage });

            return Ok(new { Status = "Success", Message = $"Transferred successfully", });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
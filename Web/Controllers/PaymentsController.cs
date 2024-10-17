using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using BankAccount = Core.Entities.BankAccount;

namespace Web.Controllers;

[Route("api/payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private static decimal chargeAmount;
    private readonly IBankAccountService _bankAccountService;

    public PaymentsController(IConfiguration configuration, IBankAccountService bankAccountService)
    {
        _configuration = configuration;
        _bankAccountService = bankAccountService;
    }


    [HttpPost("charge")]
    [Authorize]
    public async Task<IActionResult> Charge([FromBody] ChargeRequestDto chargeRequestDto)
    {

        // get user claims
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                out Guid userId))
        {
            return Unauthorized("User ID not found.");
        }

        if (!await _bankAccountService.IsUserHasBankAccount(userId))
        {
            return NotFound(
                "You must create a bank account to be able to use these services");
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(chargeRequestDto.Amount * 100), //
            Currency = "usd",
            PaymentMethod = chargeRequestDto.PaymentMethodId,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never"
            },
            Confirm = false
        };

        var service = new PaymentIntentService();
        try
        {
            // Create a PaymentIntent
            var paymentIntent = await service.CreateAsync(options);

            // Return the PaymentIntentId
            chargeAmount = chargeRequestDto.Amount;
            return Ok(new
            {
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status
            });
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
            var paymentIntent
                = await service.ConfirmAsync(confirmRequestDto.PaymentIntentId, new PaymentIntentConfirmOptions());

            // get user claims
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    out Guid userId))
            {
                return Unauthorized("User ID not found.");
            }

            if (!await _bankAccountService.IsUserHasBankAccount(userId))
            {
                return NotFound(
                    "You must create a bank account to be able to use these services");
            }

            var bankAccountDetails
                = await _bankAccountService.GetBankAccountDetailsById(userId);

            if (bankAccountDetails == null)
            {
                return Ok($"You account has been charged with {chargeAmount}\nAdditionally something went wrong while getting you bank account details");
            }


            var chargeBankResult = await _bankAccountService.ChargeAccount(userId, chargeAmount, bankAccountDetails);

            if (chargeBankResult == false)
            {
                return BadRequest("Something went wrong.");
            }

            chargeAmount = 0;
            return Ok(new
            {
                paymentIntent.Status,
                BankAccountDetails = bankAccountDetails
            });
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
}
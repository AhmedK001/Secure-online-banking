using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Web.Controllers;

[Route("api/payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public PaymentsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    [HttpPost("charge")]
    public async Task<IActionResult> Charge([FromBody] ChargeRequestDto chargeRequestDto)
    {
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
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmRequestDto confirmRequestDto)
    {
        var service = new PaymentIntentService();
        try
        {
            // Confirm the Payment
            var paymentIntent
                = await service.ConfirmAsync(confirmRequestDto.PaymentIntentId, new PaymentIntentConfirmOptions());
            return Ok(new { paymentIntent.Status });
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
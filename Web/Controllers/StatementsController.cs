using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("api/statements")]
[ApiController]
public class StatementsController : ControllerBase
{
    private readonly IClaimsService _claimsService;
    private readonly IBankAccountService _bankAccountService;
    private readonly IOperationService _operationService;

    public StatementsController(IClaimsService claimsService, IBankAccountService bankAccountService,
        IOperationService operationService)
    {
        _claimsService = claimsService;
        _bankAccountService = bankAccountService;
        _operationService = operationService;
    }

    [HttpGet("charge-account")]
    [Authorize]
    public async Task<IActionResult> GetChargeAccountLogs()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var operations = await _operationService.GetChargeAccountLogs(bankAccountDetails.AccountNumber);

            var operationsNewDto = operations.Select(o => new
            {
                o.AccountNumber,
                o.AccountId,
                o.OperationId,
                OperationType = Enum.GetName(typeof(EnumOperationType), o.OperationType),
                Currency = Enum.GetName(typeof(EnumCurrency), o.Currency),
                Amount = o.Amount.ToString("F2"),
                Date = o.DateTime.ToString("dd-MM-yyyy HH:mm:ss")
            }).OrderByDescending(o => o.Date);

            return Ok(new { Operations = operationsNewDto });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("currency-change")]
    [Authorize]
    public async Task<IActionResult> GetCurrencyChangeLogs()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));

            var operations = await _operationService.GetCurrencyChangeLogs(bankAccountDetails.AccountNumber);

            var operationsNewDto = operations.Select(o => new
            {
                o.AccountNumber,
                o.AccountId,
                o.OperationId,
                OperationType = Enum.GetName(typeof(EnumOperationType), o.OperationType),
                o.Description,
                Currency = Enum.GetName(typeof(EnumCurrency), o.Currency),
                Date = o.DateTime.ToString("dd-MM-yyyy HH:mm:ss"),
            }).OrderByDescending(o => o.Date);

            return Ok(new { Operations = operationsNewDto });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("transactions-to-card")]
    [Authorize]
    public async Task<IActionResult> GetTransactionsToCardLogs()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var operations = await _operationService.GetTransactionsToCardLogs(bankAccountDetails.AccountNumber);

            var operationsNewDto = operations.Select(o => new
            {
                o.AccountNumber,
                o.AccountId,
                o.OperationId,
                OperationType = Enum.GetName(typeof(EnumOperationType), o.OperationType),
                o.Description,
                Amount = o.Amount.ToString("F2"),
                Currency = Enum.GetName(typeof(EnumCurrency), o.Currency),
                Date = o.DateTime.ToString("dd-MM-yyyy HH:mm:ss"),
            }).OrderByDescending(o => o.Date);

            return Ok(new { Operations = operationsNewDto });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
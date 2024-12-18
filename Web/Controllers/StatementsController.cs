using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("api/statements")]
[ApiController]
public class StatementsController : ControllerBase
{
    private readonly IClaimsService _claimsService;
    private readonly IBankAccountService _bankAccountService;
    private readonly IOperationService _operationService;
    private readonly UserManager<User> _userManager;
    private readonly IOperationsRepository _operationsRepository;

    public StatementsController(IOperationsRepository operationsRepository,UserManager<User> userManager,IClaimsService claimsService, IBankAccountService bankAccountService,
        IOperationService operationService)
    {
        _claimsService = claimsService;
        _bankAccountService = bankAccountService;
        _operationService = operationService;
        _userManager = userManager;
        _operationsRepository = operationsRepository;
    }

    [HttpGet("account-history")]
    [Authorize]
    public async Task<IActionResult> GetAllOperations()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var operations = await _operationService.GetAllLogs(bankAccountDetails.AccountNumber,36);
            if (!operations.Any() || operations is null)
            {
                return BadRequest(new { ErrorMessage = "No records found." });
            }

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
            });

            return Ok(new { Operations = operationsNewDto });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("transactions")]
    [Authorize]
    public async Task<IActionResult> GetTransactions()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var operations = await _operationsRepository.GetTransactionLogs(bankAccountDetails.AccountNumber);
            if (!operations.Any() || operations is null)
            {
                return BadRequest(new { ErrorMessage = "No records found." });
            }
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
            });

            return Ok(new { Operations = operationsNewDto });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("stocks")]
    [Authorize]
    public async Task<IActionResult> GetStockLogs()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var operations = await _operationsRepository.GetStockLogs(bankAccountDetails.AccountNumber);
            if (!operations.Any() || operations is null)
            {
                return BadRequest(new { ErrorMessage = "No records found." });
            }
            var operationsNewDto = operations.Select(o => new
            {
                o.AccountNumber,
                o.AccountId,
                o.OperationId,
                OperationType = Enum.GetName(typeof(EnumOperationType), o.OperationType),
                o.Description,
                TotalAmount = o.Amount.ToString("F2"),
                Currency = Enum.GetName(typeof(EnumCurrency), o.Currency),
                Date = o.DateTime.ToString("dd-MM-yyyy HH:mm:ss"),
            });

            return Ok(new { Operations = operationsNewDto });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("exchanges")]
    [Authorize]
    public async Task<IActionResult> GetExchangeLogs()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var operations = await _operationsRepository.GetExchangeLogs(bankAccountDetails.AccountNumber);
            if (!operations.Any() || operations is null)
            {
                return BadRequest(new { ErrorMessage = "No records found." });
            }
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
            });

            return Ok(new { Operations = operationsNewDto });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
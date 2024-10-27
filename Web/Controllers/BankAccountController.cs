using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/bank-accounts")]
public class BankAccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IBankAccountService _bankAccountService;
    private readonly IClaimsService _claimsService;

    public BankAccountController(UserManager<User> userManager, SignInManager<User> signInManager,
        IBankAccountService bankAccountService, IClaimsService claimsService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _bankAccountService = bankAccountService;
        _claimsService = claimsService;
    }

    [HttpPost("bank-account")]
    [Authorize]
    public async Task<IActionResult> CreateBankAccount()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);

            if (await _bankAccountService.IsUserHasBankAccount(Guid.Parse(userId)))
            {
                return Conflict("Only one bank account is allowed for you");
            }

            var bankAccount = await _bankAccountService.CreateBankAccount(Guid.Parse(userId));

            if (bankAccount == null)
            {
                return BadRequest("Something went wrong.");
            }

            return Ok(new
            {
                Message = "Your bank account has been created successfully.",
                bankAccount.AccountNumber,
                bankAccount.Balance,
                bankAccount.CreationDate,
                bankAccount.UserId
            });
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }

    [HttpGet("bank-account")]
    public async Task<IActionResult> GetBankAccountDetails()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            return Ok(new
            {
                BankAccount = new
                {
                    bankAccountDetails.AccountNumber,
                    bankAccountDetails.CreationDate,
                    Currency = Enum.GetName(typeof(EnumCurrency), bankAccountDetails.Currency),
                    bankAccountDetails.Balance,
                }
            });
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }

    [HttpPut("change-currency")]
    [Authorize]
    public async Task<IActionResult> ChangeCurrency([FromBody] string currencySymbolDto)
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));

            if (!Enum.TryParse(currencySymbolDto, out EnumCurrency currencySymbol))
            {
                var availableSymbols = string.Join(", ", Enum.GetNames(typeof(EnumCurrency)));
                return BadRequest(
                    $"Currency symbol not found. Available currencies to use are: {availableSymbols}");
            }

            await _bankAccountService.ChangeCurrencyAsync(currencySymbol, bankAccountDetails.AccountNumber);

            var accountAfterCurrencyChanged
                = await _bankAccountService.GetDetailsByAccountNumber(bankAccountDetails.AccountNumber);

            return Ok(new
            {
                Message = "Your Bank Account currency changed successfully.",
                BankAccount = new
                {
                    accountAfterCurrencyChanged.AccountNumber,
                    accountAfterCurrencyChanged.CreationDate,
                    Currency = Enum.GetName(typeof(EnumCurrency), accountAfterCurrencyChanged.Currency),
                    accountAfterCurrencyChanged.Balance
                }
            });
        }
        catch (Exception e)
        {
            var errorMessage = e.InnerException?.Message ?? e.Message;

            return BadRequest(new
            {
                Message = errorMessage
            });
        }
    }
}
using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Route("api/bank-accounts")]
public class BankAccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IBankAccountService _bankAccountService;
    private readonly IClaimsService _claimsService;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;

    public BankAccountController(IEmailBodyBuilder emailBodyBuilder,IEmailService emailService,IConfiguration configuration,UserManager<User> userManager, SignInManager<User> signInManager,
        IBankAccountService bankAccountService, IClaimsService claimsService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _bankAccountService = bankAccountService;
        _claimsService = claimsService;
        _configuration = configuration;
        _emailService = emailService;
        _emailBodyBuilder = emailBodyBuilder;
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
                return BadRequest("Unexpected error happened.");
            }

            return Ok(new
            {
                Message = "Your bank account has been created successfully.",
                bankAccount.AccountNumber,
                Currency = Enum.GetName(typeof(EnumCurrency), bankAccount.Currency),
                bankAccount.Balance,
                bankAccount.CreationDate,
                bankAccount.UserId
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("bank-account")]
    [Authorize]
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
                    CreationDate = bankAccountDetails.CreationDate.ToString("dd-MM-yyyy"),
                    Currency = Enum.GetName(typeof(EnumCurrency), bankAccountDetails.Currency),
                    bankAccountDetails.Balance,
                }
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("change-currency")]
    [Authorize]
    public async Task<IActionResult> ChangeCurrency([FromBody] CurrencySymbolDto currencySymbolDto)
    {
        try
        {
            // User cannot exchange directly from, or to EGP, TRY, SAR, AED until convert to USD or EUR, then convert to the target

            // IF ACCOUNT GOT MONEY SO ASK FOR EXCHANGE BEFORE CHANGING CURRENCY
            var userId = await _claimsService.GetUserIdAsync(User);
            var bankAccountDetails = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

            if (!Enum.TryParse(currencySymbolDto.Symbol, out EnumCurrency currencySymbol))
            {
                var availableSymbols = string.Join(", ", Enum.GetNames(typeof(EnumCurrency)));
                return BadRequest(
                    $"Currency symbol not found. Available currencies to use are: {availableSymbols}");
            }

            if (bankAccountDetails.Currency == currencySymbol)
            {
                return BadRequest("Your account uses the same currency already.");
            }

            // If current currency is AED or SAR and target is AED or SAR, so it can be converted directly
            bool letThemPass = ((Enum.GetName(typeof(EnumCurrency), bankAccountDetails.Currency) == "SAR") ||
                                Enum.GetName(typeof(EnumCurrency), bankAccountDetails.Currency) == "AED") &&
                               (currencySymbolDto.Symbol == "AED" || currencySymbolDto.Symbol == "SAR");

            // Any else must contain USD or EUR as current account currency or USD or EUR as a target
            if (letThemPass == false &&
                !(bankAccountDetails.Currency == EnumCurrency.USD ||
                 bankAccountDetails.Currency == EnumCurrency.EUR || currencySymbol == EnumCurrency.USD ||
                                                                      currencySymbol == EnumCurrency.EUR))
            {
                return BadRequest($"You must exchange to USD or AED then exchange to {currencySymbolDto}");
            }


            await _bankAccountService.ChangeCurrencyAsync(currencySymbol, bankAccountDetails.AccountNumber);

            var accountAfterCurrencyChanged
                = await _bankAccountService.GetDetailsByAccountNumber(bankAccountDetails.AccountNumber);

            var htmlResponse
                = _emailBodyBuilder.ChangeCurrencyBankHtmlResponse("Bank Account Currency Change.",accountAfterCurrencyChanged);
            await _emailService.SendEmailAsync(user, "Currency of your bank account has updated",htmlResponse);

            return Ok(new
            {
                Message = "Your Bank Account currency changed successfully.",
                BankAccount = new
                {
                    accountAfterCurrencyChanged.AccountNumber,
                    CreationDate = accountAfterCurrencyChanged.CreationDate.ToString("dd-MM-yyyy"),
                    Currency = Enum.GetName(typeof(EnumCurrency),
                        accountAfterCurrencyChanged.Currency),
                    accountAfterCurrencyChanged.Balance
                }
            });
        }
        catch (Exception e)
        {
            return BadRequest(new { e.Message });
        }
    }
}
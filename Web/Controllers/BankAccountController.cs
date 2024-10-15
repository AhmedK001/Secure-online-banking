using System.Security.Claims;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
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

    public BankAccountController(UserManager<User> userManager,
        SignInManager<User> signInManager, IBankAccountService bankAccountService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _bankAccountService = bankAccountService;
    }

    [HttpPost("bank-account")]
    [Authorize]
    public async Task<IActionResult> CreateBankAccount()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized("You are not logged in.");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null)
        {
            return Unauthorized("You are not logged in");
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Invalid user ID.");
        }

        if (await _bankAccountService.IsUserHasBankAccount(userId))
        {
            return Conflict("Only one bank account is allowed for you");
        }

        var bankAccount = await _bankAccountService.CreateBankAccount(userId);

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

}
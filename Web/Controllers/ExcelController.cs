using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/excel")]
public class ExcelController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IClaimsService _claimsService;
    private readonly IExcelService _excelService;
    private readonly IBankAccountService _bankAccountService;
    private readonly ICardsService _cardsService;
    private readonly IOperationService _operationService;

    public ExcelController(IOperationService operationService,ICardsService cardsService, IBankAccountService bankAccountService, IExcelService excelService, UserManager<User> userManager, IClaimsService claimsService)
    {
        _excelService = excelService;
        _userManager = userManager;
        _claimsService = claimsService;
        _bankAccountService = bankAccountService;
        _cardsService = cardsService;
        _operationService = operationService;
    }

    [HttpGet("statements/{periodAsMonth:int}")]
    [Authorize]
    public async Task<IActionResult> GetBankStatements(int periodAsMonth)
    {

        try
        {
            if (periodAsMonth < 1)
                return BadRequest(new { ErrorMessage = "Minimum period allowed is 1 month" });

            if (periodAsMonth > 36)
                return BadRequest(new { ErrorMessage = "Maximum period allowed is 36 month" });

            var userId = await _claimsService.GetUserIdAsync(User);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            var bankAccount = await _bankAccountService.GetDetailsById(Guid.Parse(userId));
            var operations = await _operationService.GetAllLogs(bankAccount.AccountNumber, periodAsMonth);

            var streamContent = await _excelService.GetAllOperations(operations, user, bankAccount);

            // read the stream form stream content
            var memoryStream = await streamContent.ReadAsStreamAsync();

            // Return the file
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MyBankStatements.xlsx");
        }
        catch (Exception e)
        {
            return BadRequest("Unexpected error happened!");
        }
    }
}
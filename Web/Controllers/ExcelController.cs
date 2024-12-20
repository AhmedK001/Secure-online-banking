using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Authorize]
[Route("api/excel")]
public class ExcelController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IClaimsService _claimsService;
    private readonly IExcelService _excelService;
    private readonly IBankAccountService _bankAccountService;
    private readonly ICardsService _cardsService;
    private readonly IOperationService _operationService;
    private readonly IEmailService _emailService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;

    public ExcelController(IEmailBodyBuilder emailBodyBuilder, IEmailService emailService,
        IOperationService operationService, ICardsService cardsService,
        IBankAccountService bankAccountService, IExcelService excelService, UserManager<User> userManager,
        IClaimsService claimsService)
    {
        _excelService = excelService;
        _userManager = userManager;
        _claimsService = claimsService;
        _bankAccountService = bankAccountService;
        _cardsService = cardsService;
        _operationService = operationService;
        _emailService = emailService;
        _emailBodyBuilder = emailBodyBuilder;
    }

    [HttpGet("statements/{number-of-months:int}/{send-email:bool}")]

    public async Task<IActionResult> GetBankStatements(
        [FromRoute(Name = "number-of-months")] int periodAsMonth,
        [FromRoute(Name = "send-email")] bool sendEmil)
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
            if (!operations.Any()) return BadRequest(new { Message = "No operations found." });

            StreamContent streamContent;
            streamContent = await _excelService.GetAllOperations(sendEmil, operations, user, bankAccount);

            // read the stream form stream content
            var memoryStream = await streamContent.ReadAsStreamAsync();
            var stream = Stream.Synchronized(memoryStream);

            var file = File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MyBankStatements.xlsx");
            if (sendEmil)
            {
                var htmlResponse = _emailBodyBuilder.SingleMessageHtmlResponse("Your bank statements",
                    "Enjoy checking your bank statements.", $"{user.FirstName} {user.LastName}");

                await _emailService.SendEmailAsync(user, "Your Bank-Account Statements", htmlResponse, stream,
                    "MyBankStatements.xlsx");
                return Ok(new {Status = "Success",Message = "Check your email address."});
            }

            // Return the file
            return file;
        }
        catch (Exception e)
        {
            return BadRequest("Unexpected error happened!");
        }
    }
}
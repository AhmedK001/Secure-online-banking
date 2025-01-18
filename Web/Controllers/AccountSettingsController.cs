using System.Security.Claims;
using System.Web;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
public class AccountSettingsController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IClaimsService _claimsService;
    private readonly IAccountSettingsService _accountSettingsService;
    private readonly IEmailService _emailService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;
    private readonly IUpdatePassword _updatePassword;

    public AccountSettingsController(IUpdatePassword updatePassword, IEmailBodyBuilder emailBodyBuilder,
        IEmailService emailService, UserManager<User> userManager,
        IAccountSettingsService accountSettingsService, IClaimsService claimsService)
    {
        _accountSettingsService = accountSettingsService;
        _claimsService = claimsService;
        _userManager = userManager;
        _emailService = emailService;
        _emailBodyBuilder = emailBodyBuilder;
        _updatePassword = updatePassword;
    }


    /// <summary>
    /// Disable notifications for your account movements.
    /// </summary>
    /// <returns></returns>
    [HttpGet("notifications/disable")]
    public async Task<IActionResult> DisableNotifications()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return BadRequest("No users found");

            var result = await _accountSettingsService.DisableNotificationsAsync(user);
            if (!result.isSuccess) return BadRequest(new { ErrorMessage = result.errorMessage });

            return Ok(new { Status = "Success", Message = "Notifications disabled successfully." });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Enable notifications for your account movements.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet("notifications/enable")]
    public async Task<IActionResult> EnableNotifications()
    {
        try
        {
            var userId = await _claimsService.GetUserIdAsync(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return BadRequest("No users found");

            var result = await _accountSettingsService.EnableNotificationsAsync(user);
            if (!result.isSuccess) return BadRequest(new { ErrorMessage = result.errorMessage });

            return Ok(new { Status = "Success", Message = "Notifications enabled successfully." });
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Request a password reset. This can be if you are not logged in.
    /// </summary>
    /// <param name="emailDto"></param>
    /// <returns></returns>
    [HttpPost("passwords/request-reset")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestResetPassword([FromBody] EmailDto emailDto)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok(new { ErrorMessage = "You cannot request password reset while logged in." });
        }

        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }

        var user = await _userManager.FindByNameAsync(emailDto.Email);

        if (user is null) return BadRequest(new { ErrorMessage = "No users found." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetUrl = Url.Action("ResetPassword", "AccountSettings", new { token }, Request.Scheme);
        Console.WriteLine(token);

            var body = _emailBodyBuilder.PasswordResetHtmlResponse("Password Reset Request.", emailDto.Email,
                resetUrl);

            await _emailService.ForceSendEmailAsync(user, "Password Reset Request.", body);

        return Ok(new { Status = "Success", Message = "Reset password link sent to your email address." });
    }

    /// <summary>
    /// Reset your password using the token received via email.
    /// </summary>
    /// <param name="passwordDto"></param>
    /// <returns></returns>
    [HttpPut("passwords/reset")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto passwordDto)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok(new { Message = "You are already logged in." });
        }

        var user = await _userManager.FindByNameAsync(passwordDto.Email);
        if (user is null) return BadRequest(new { ErrorMessage = "No users found." });

        var decodedToken = HttpUtility.UrlDecode(passwordDto.Token);

        if (!await _userManager.VerifyUserTokenAsync(user,
                _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", decodedToken))
        {
            return BadRequest(new { ErrorMessage = "Invalid Token." });
        }

        if (await _userManager.CheckPasswordAsync(user, passwordDto.Password))
        {
            return BadRequest(new { ErrorMessage = "Cannot use same old password." });
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, passwordDto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // if account locked-out => unlock it
        if (await _userManager.IsLockedOutAsync(user))
        {
            await _userManager.SetLockoutEnabledAsync(user, false);
        }

        var htmlResponse = _emailBodyBuilder.SingleMessageHtmlResponse("Password Changed",
            "Your account password has been reset, If it is not you contact us immediately", $"{user.FirstName} {user.LastName}");
        await _emailService.ForceSendEmailAsync(user, "Your account password has been changed", htmlResponse);

        return Ok(new { Status = "Success", Message = "Your password has been reset successfully." });
    }

    /// <summary>
    /// Update your account password. Requires authentication.
    /// </summary>
    /// <param name="_updatePasswordDto"></param>
    /// <returns></returns>
    [HttpPut("passwords/password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto _updatePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }

        if (_updatePasswordDto.CurrentPassword == _updatePasswordDto.Password)
        {
            return BadRequest(new { ErrorMessage = "Current and new password cannot be the same." });
        }

        // get user id
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User not found.");

        // get user object
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("User not found.");

        // check old password if matches current one.
        if (await _userManager.CheckPasswordAsync(user, _updatePasswordDto.Password))
        {
            return BadRequest(new { ErrorMessage = "Invalid password." });
        }

        var result = await _updatePassword.UpdatePasswordAsync(user, _updatePasswordDto);

        if (!result.isSuccess)
        {
            return BadRequest(new {ErrorMessage = result.errorMessage});
        }

        var htmlResponse = _emailBodyBuilder.SingleMessageHtmlResponse("Password Changed",
            "Your account password has been changed, If it is not you reset password immediately", $"{user.FirstName} {user.LastName}");

        await _emailService.ForceSendEmailAsync(user, "Your account password has been changed", htmlResponse);

        return Ok(new {SuccessMessage = "Your password has been updated successfully."});
    }
}
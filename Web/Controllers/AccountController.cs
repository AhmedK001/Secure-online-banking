using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.RegistrationDTOs;
using Application.DTOs.ResponseDto;
using Application.Interfaces;
using Application.Mappers;
using Application.Validators;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IRegistrationService _registrationService;
    private readonly ISearchUserService _searchUserService;
    private readonly IJwtService _jwtService;
    private readonly IUpdatePassword _updatePassword;
    private readonly ITwoFactorAuthService _twoFactorAuthService;
    private readonly IEmailService _emailService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;
    private readonly IClaimsService _claimsService;


    public AccountController(IClaimsService claimsService,IEmailService emailService, IEmailBodyBuilder emailBodyBuilder,
        ITwoFactorAuthService twoFactorAuthService, IUpdatePassword updatePassword, IJwtService jwtService,
        UserManager<User> userManager, SignInManager<User> signInManager, IRegistrationService registrationService,
        ISearchUserService searchUserService)
    {
        _userManager = userManager;
        _registrationService = registrationService;
        _searchUserService = searchUserService;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _updatePassword = updatePassword;
        _twoFactorAuthService = twoFactorAuthService;
        _emailService = emailService;
        _emailBodyBuilder = emailBodyBuilder;
        _claimsService = claimsService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterNewUser([FromBody] RegisterUserDto? userDto)
    {
        // Check if userDto is null
        if (userDto == null)
        {
            return BadRequest("User data is required.");
        }

        // Main validation for userDTO data
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if received data is unique
        if (!_searchUserService.CheckIfNationalIdUnique(userDto.NationalId))
        {
            return BadRequest(new { Message = "National ID number is used before." });
        }

        if (!_searchUserService.CheckIfEmailUnique(userDto.Email))
        {
            return BadRequest(new { Message = "Email is used before." });
        }

        if (!_searchUserService.CheckIfPhoneUnique(userDto.PhoneNumber))
        {
            return BadRequest(new { Message = "Phone number is used before." });
        }

        // date of birth validation
        var birthDateValidationResult = UserInfoValidator.IsAcceptedBirthDate(userDto.DateOfBirth);
        if (birthDateValidationResult != ValidationResult.Success)
        {
            return BadRequest(birthDateValidationResult.ErrorMessage);
        }

        // Convert userDto to user object
        User user = ConvertToSomeObject.ConvertToUserObject(userDto);
        {
            user.UserName = userDto.Email;
        }

        // Register User using UserManager
        var registrationResult = await _userManager.CreateAsync(user, userDto.Password);
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Account",
            new { Email = userDto.Email, token = token }, Request.Scheme);

        var userName = $"{user.FirstName} {user.LastName}";
        var body = _emailBodyBuilder.EmailConfirmationHtmlResponse("Confirm your email", userName, confirmationLink);
        await _emailService.SendEmailAsync(user.UserName, "Confirm your email", body);

        return Ok(new { message = "Registration successful. Please check your email for confirmation." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        if (email == null || token == null)
        {
            return BadRequest("Invalid email confirmation request.");
        }

        var user = await _userManager.FindByNameAsync(email);
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        if (user.EmailConfirmed)
        {
            return BadRequest("Your email has been confirmed before.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            return Ok("Email confirmed successfully.");
        }

        return BadRequest("Error confirming email.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginDto userLoginDto)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok(new { Message = "You are already logged in." });
        }

        var normalizedEmail = userLoginDto.EmailAddress.ToLower();

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            return Unauthorized("Email or password is incorrect.");
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return Unauthorized("Please confirm your email address before logging in.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return BadRequest(new
            {
                ErrorMessage = "Your account has been locked due to many invalid logins.",
                Solution = "Reset your password."
            });
        }

        if (!await _userManager.CheckPasswordAsync(user,userLoginDto.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized("Email or password is incorrect.");
        }
        var passwordLoginResult = await _signInManager.PasswordSignInAsync(
            user.UserName, userLoginDto.Password, isPersistent: false,
            lockoutOnFailure: false);

        await _userManager.ResetAccessFailedCountAsync(user);
        _twoFactorAuthService.RemoveCode(user.Id.ToString());
        int expirationTimeInMinutes = 5;
        var userName = $"{user.FirstName} {user.LastName}";
        var code = _twoFactorAuthService.Generate2FaCode(user.Id.ToString(),expirationTimeInMinutes);
        Console.WriteLine(code);
        Console.WriteLine(code);
        var body = _emailBodyBuilder.TwoFactorAuthHtmlResponse("Your Two-Factor auth code for login", userName, code,
            expirationTimeInMinutes);
        await _emailService.SendEmailAsync(user.UserName, "Two-Factor auth code", body);

        return Unauthorized(new { status = StatusCode(401), Message = "2FA code sent to your email address." });
    }

    [HttpPost("verify-2fa-code")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTwoFactorCode(TwoFactorVerificationDto verificationDto)
    {
        try
        {
            var email = verificationDto.EmailAddress.ToLower();
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user is null) return BadRequest(new{ErrorMessage = "No users found."});
            var userId = user.Id.ToString();
            var code = _twoFactorAuthService.GetStoredCode(userId);

            if (code == null)
            {
                return Unauthorized(new { ErrorMessage = "Verification code is invalid." });
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return BadRequest(new
                {
                    ErrorMessage = "Your account has been locked due to many invalid logins.",
                    Solution = "Reset your password."
                });
            }

            if (!_twoFactorAuthService.IsValidTwoFactorCode(user.Id.ToString(), verificationDto.Code))
            {
                return Unauthorized(new { ErrorMessage = "Email, verification code or both are invalid." });
            }

            _twoFactorAuthService.RemoveCode(userId);
            var token = _jwtService.CreateJwtToken(user);
            return Ok(new
            {
                token.Token,
                token.ExpirationTime,
                Message = "Successfully signed in.",
            });
        }
        catch (Exception e)
        {
            return BadRequest("Email, verification code or both are invalid.");
        }
    }

    [HttpPost("request-password-reset")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestResetPassword([FromBody] string email)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }

        var user = await _userManager.FindByNameAsync(email);

        if (user is null) return BadRequest(new{ErrorMessage = "No users found."});

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetUrl = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);

        var body = _emailBodyBuilder.PasswordResetHtmlResponse("Password Reset Request.", email, resetUrl);

        await _emailService.SendEmailAsync(email, "Password Reset Request.", body);

        return Ok(new { Status = "Success", Message = "Reset password link sent to your email address." });
    }

    [HttpPut("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto passwordDto)
    {
        var user = await _userManager.FindByNameAsync(passwordDto.Email);
        if (user is null) return BadRequest(new{ErrorMessage = "No users found."});

        var result = await _userManager.ResetPasswordAsync(user, passwordDto.Token, passwordDto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // if account locked-out => unlock it
        if (await _userManager.IsLockedOutAsync(user))
        {
            await _userManager.SetLockoutEnabledAsync(user, false);
        }
        return Ok(new { Status = "Success", Message = "Your password has been reset successfully." });
    }

    [HttpPut("update-password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto _updatePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }

        // get user id
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User not found.");

        // get user object
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("User not found.");

        // check old password if matches current one.
        var callUpdatePasswordServiceMethodResult = _updatePassword.UpdatePasswordAsync(user, _updatePasswordDto);

        if (!callUpdatePasswordServiceMethodResult.Result.Succeeded)
        {
            return BadRequest(callUpdatePasswordServiceMethodResult.Result);
        }

        if (callUpdatePasswordServiceMethodResult.Result.Succeeded)
        {
            return Ok("Your password has been updated successfully.");
        }

        return Unauthorized("You are not logged in.");
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized("You are not logged in.");
        }

        var logoutResult = _signInManager.SignOutAsync();
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!logoutResult.IsCompletedSuccessfully)
        {
            return BadRequest("Something went wrong.");
        }

        return Ok(new { Message = "You have logged out successfully. If using swagger logout manually." });
    }
}
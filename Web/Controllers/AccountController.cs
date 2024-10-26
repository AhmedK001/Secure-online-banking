using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.RegistrationDTOs;
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

    public AccountController(IUpdatePassword updatePassword,
        IJwtService jwtService, UserManager<User> userManager,
        SignInManager<User> signInManager,
        IRegistrationService registrationService,
        ISearchUserService searchUserService)
    {
        _userManager = userManager;
        _registrationService = registrationService;
        _searchUserService = searchUserService;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _updatePassword = updatePassword;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterNewUser(
        [FromBody] RegisterUserDto? userDto)
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
            return BadRequest(new
            {
                Message = "This National ID number is used before."
            });
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
        var birthDateValidationResult
            = UserInfoValidator.IsAcceptedBirthDate(userDto.DateOfBirth);
        if (birthDateValidationResult != ValidationResult.Success)
        {
            return BadRequest(birthDateValidationResult.ErrorMessage);
        }

        // Convert userDto to user object
        User user = ConvertToSomeObject.ConvertToUserObject(userDto);
        {
            user.UserName = userDto.Email;
        }
        ;

        // Register User using UserManager
        var registrationResult
            = await _userManager.CreateAsync(user, userDto.Password);

        if (registrationResult.Succeeded)
        {
            return Ok(new { Message = "User registered successfully." });
        }

        // Return the errors if registration failed
        return BadRequest(registrationResult.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginDto userLoginDto)
    {
        // Normalize the email for comparison
        var normalizedEmail = userLoginDto.EmailAddress.ToLower();

        // Try to find the user by normalized email
        var user
            = await _userManager.Users.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == normalizedEmail);

        // Check if user was found
        if (user == null)
        {
            return Unauthorized("Email or password is incorrect.");
        }

        // Attempt to sign the user in using email and password
        var passwordLoginResult = await _signInManager.PasswordSignInAsync(
            user.UserName, userLoginDto.Password, isPersistent: false,
            lockoutOnFailure: false);

        if (passwordLoginResult.IsLockedOut)
        {
            return BadRequest("Your account is locked out.");
        }

        if (passwordLoginResult.RequiresTwoFactor)
        {
            return BadRequest("Two-factor authentication is required.");
        }

        if (passwordLoginResult.Succeeded)
        {
            var userPhoneNumberFromDb
                = await _userManager.Users.FirstOrDefaultAsync(u =>
                    u.Id == user.Id);

            var token = _jwtService.CreateJwtToken(user);

            return Ok(new
            {
                token.Token,
                token.ExpirationTime,
                Message = "Successfully signed in.",
                PhoneNumber = userPhoneNumberFromDb?.PhoneNumber,
            });
        }

        return BadRequest("Email or password is incorrect.");
    }

    [HttpPut("update-password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword(
        [FromBody] UpdatePasswordDto _updatePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }

        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized("You are not logged in.");
        }

        // get user id
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User not found.");

        // get user object
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("User not found.");

        // check old password if matches current one.
        var callUpdatePasswordServiceMethodResult
            = _updatePassword.UpdatePasswordAsync(user, _updatePasswordDto);

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

        if (!logoutResult.IsCompletedSuccessfully)
        {
            return BadRequest("Something went wrong.");
        }

        return Ok(new { Message = "You have logged out successfully." });
    }
}
using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.DTOs.RegistrationDTOs;
using Application.Interfaces;
using Application.Mappers;
using Application.Validators;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IRegistrationService _registrationService;
    private readonly ISearchUserService _searchUserService;
    private readonly IIbanGeneratorService _ibanGeneratorService;
    private readonly IJwtService _jwtService;

    public AccountController(IJwtService jwtService,UserManager<User> userManager, SignInManager<User> signInManager,IRegistrationService registrationService, IIbanGeneratorService ibanGeneratorService,ISearchUserService searchUserService)
    {
        _userManager = userManager;
        _registrationService = registrationService;
        _ibanGeneratorService = ibanGeneratorService;
        _searchUserService = searchUserService;
        _signInManager = signInManager;
        _jwtService = jwtService;
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
            return BadRequest(new { Message = "This National ID number is used before." });
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
        User user = UserMapper.ConvertToUserObject(userDto);
        {
            user.UserName = userDto.Email;
        };

        // Register User using UserManager
        var registrationResult = await _userManager.CreateAsync(user, userDto.Password);

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
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        // Check if user was found
        if (user == null)
        {
            return BadRequest("Email or password is incorrect.");
        }

        // Attempt to sign the user in using email and password
        var passwordLoginResult = await _signInManager.PasswordSignInAsync(user.UserName, userLoginDto.Password, isPersistent: false, lockoutOnFailure: false);

        if (passwordLoginResult.Succeeded)
        {
            var authenticationResponse = _jwtService.CreateJwtToken(user);

            var userPhoneNumberFromDb = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            return Ok(new
            {
                Message = "Successfully signed in.",
                PhoneNumber = userPhoneNumberFromDb?.PhoneNumber,
                authenticationResponse = authenticationResponse,
            });
        }
        else if (passwordLoginResult.IsLockedOut)
        {
            return BadRequest("Your account is locked out.");
        }
        else if (passwordLoginResult.RequiresTwoFactor)
        {
            return BadRequest("Two-factor authentication is required.");
        }
        else
        {
            return BadRequest("Email or password is incorrect.");
        }
    }



}
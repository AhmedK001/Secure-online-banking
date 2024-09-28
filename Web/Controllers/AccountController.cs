using System.ComponentModel.DataAnnotations;
using Application.DTOs.RegistrationDTOs;
using Application.Interfaces;
using Application.Mappers;
using Application.Validators;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/registration")]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IRegistrationService _registrationService;
    private readonly ISearchUserService _searchUserService;
    private readonly IIbanGeneratorService _ibanGeneratorService;

    public AccountController(UserManager<User> userManager,IRegistrationService registrationService, IIbanGeneratorService ibanGeneratorService,ISearchUserService searchUserService)
    {
        _userManager = userManager;
        _registrationService = registrationService;
        _ibanGeneratorService = ibanGeneratorService;
        _searchUserService = searchUserService;
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

}
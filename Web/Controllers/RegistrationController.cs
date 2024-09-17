using System.ComponentModel.DataAnnotations;
using Application.DTOs.RegistrationDTOs;
using Application.Interfaces;
using Application.Mappers;
using Microsoft.AspNetCore.Mvc;
using Application.Validators;
using Core.Entities;
using Core.Interfaces;

public class UserController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly IIbanGeneratorService _ibanGeneratorService;

    public UserController(IRegistrationService registrationService, IIbanGeneratorService ibanGeneratorService)
    {
        _registrationService = registrationService;
        _ibanGeneratorService = ibanGeneratorService;
    }
    /*
    [HttpPost("register")]
    public IActionResult RegisterNewUser([FromBody] RegisterUserDTO? userDto)
    {
        //
        // userDto gets null when entring different formmat (20210-09-02)
        //

        // first validation for userDTO data
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Convert userDto to user object
        User user = UserMapper.ConvertToUser(userDto);

        // Validate user details
        ValidationResult validationResult
            = UserInfoValidator.IsUserInfoAccepted(user);

        if (validationResult != ValidationResult.Success)
        {
            return BadRequest(validationResult.ErrorMessage);
        }

        // Register User
        var registrationResult = _registrationService.RegisterUser(user);
        var ibanResult = _ibanGeneratorService.GenerateIban(user.NationalId);


        return Ok(new { IbanMessage = ibanResult , Message = registrationResult});
    }*/

    [HttpPost("register")]
    public IActionResult RegisterNewUser([FromBody] RegisterUserDTO? userDto)
    {
        //
        // userDto gets null when entring different formmat (20210-09-02)
        //

        // first validation for userDTO data
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Convert userDto to user object
        User user = UserMapper.ConvertToUser(userDto);

        // Validate user details
        //ValidationResult validationResult
          //  = UserInfoValidator.IsUserInfoAccepted(user);

        //if (validationResult != ValidationResult.Success)
        //{
        //    return BadRequest(validationResult.ErrorMessage);
        //}

        // Register User
        var registrationResult = _registrationService.RegisterUserAsync(user);
        //var ibanResult = _ibanGeneratorService.GenerateIban(user.NationalId);


        return Ok(new { Message = registrationResult });
    }
}
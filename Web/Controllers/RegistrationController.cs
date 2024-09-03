using System.ComponentModel.DataAnnotations;
using Application.DTOs.RegistrationDTOs;
using Application.Interfaces;
using Application.Mappers;
using Microsoft.AspNetCore.Mvc;
using Application.Validators;
using Core.Entities;

public class UserController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public UserController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

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


        return Ok(new { Message = registrationResult });
    }
}
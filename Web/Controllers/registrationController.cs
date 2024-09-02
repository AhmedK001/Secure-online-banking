using System.ComponentModel.DataAnnotations;
using Application.DTOs.RegistrationDTOs;
using Application.Interfaces;
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
    public IActionResult RegisterNewUser([FromBody] RegisterUserDTO userDto)
    {
    
    }
}
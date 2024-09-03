﻿using System.Net.Http.Json;
using Application.Interfaces;
using Application.Validators;
using Core.Entities;

namespace Application.Services.RegistrationService;

public class RegistrationService : IRegistrationService
{
    public async Task<string> RegisterUser(User user)
    {
        return $"Your data has been saved successfully for user: {user}";
    }

}


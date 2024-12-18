using System.Security.Claims;
using Application.Interfaces;
using Core.Entities;
using Core.Interfaces.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AccountSettingsService : IAccountSettingsService
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    public AccountSettingsService(IUserRepository userRepository,UserManager<User> userManager)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }

    public async Task<(bool isSuccess, string errorMessage)> EnableNotificationsAsync(User user)
    {
        return await _userRepository.EnableNotificationsAsync(user);
    }

    public async Task<(bool isSuccess, string errorMessage)> DisableNotificationsAsync(User user)
    {
        return await _userRepository.DisableNotificationsAsync(user);
    }
}
using System.Security.Claims;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class ClaimsService : IClaimsService
{
    private readonly UserManager<User> _userManager;

    public ClaimsService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> GetUserIdAsync(ClaimsPrincipal useClaimsPrincipal)
    {
        var userId = useClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)
            ?.Value;
        if (userId == null)
        {
            throw new ArgumentNullException("User ID not found.");
        }

        return userId;
    }


    public async Task<string> GetUserEmailAddressAsync(ClaimsPrincipal userclaimsPrincipal)
    {
        var userEmail = userclaimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

        if (userEmail == null)
        {
            throw new ArgumentNullException("User Email not found");
        }

        return userEmail;
    }
}
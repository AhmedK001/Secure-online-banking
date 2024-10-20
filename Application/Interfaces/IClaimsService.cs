using System.Security.Claims;

namespace Application.Interfaces;

public interface IClaimsService
{
    Task<string> GetUserIdAsync(ClaimsPrincipal userClamsPrincipal);
    Task<string> GetUserEmailAddressAsync(ClaimsPrincipal userClamsPrincipal);
}
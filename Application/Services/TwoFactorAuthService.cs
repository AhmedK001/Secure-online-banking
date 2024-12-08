using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

public class TwoFactorAuthService : ITwoFactorAuthService
{
    private readonly IMemoryCache _memoryCache;

    public TwoFactorAuthService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public string GenerateRandomCode()
    {
        Random random = new Random();
        return random.Next(10000, 80000).ToString();
    }

    public void StoreCode(string userId, string code,int expirationInMins)
    {
        _memoryCache.Set(userId, code, TimeSpan.FromMinutes(expirationInMins));
    }

    public bool IsValidTwoFactorCode(string userId, string code)
    {
        if (_memoryCache.TryGetValue(userId, out string storedCode))
        {
            if (storedCode == code)
            {
                return true;
            }
        }

        return false;
    }

    public string Generate2FaCode(string userId,int expirationInMins)
    {
        var code = GenerateRandomCode();
        StoreCode(userId, code,expirationInMins);
        return code;
    }

    public string? GetStoredCode(string userId)
    {
        if (_memoryCache.TryGetValue(userId, out string code))
        {
            return code;
        }

        return null;
    }

    public void RemoveCode(string userId)
    {
        _memoryCache.Remove(userId);
    }
}
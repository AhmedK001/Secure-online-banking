namespace Application.Interfaces;

public interface ITwoFactorAuthService
{
    string GenerateRandomCode();
    void StoreCode(string userId, string code,int expirationInMins);
    bool IsValidTwoFactorCode(string userId, string code);
    string Generate2FaCode(string userId, int expirationInMins);
    string? GetStoredCode(string userId);
    void RemoveCode(string userId);
}
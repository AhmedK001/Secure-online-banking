using Core.Entities;

namespace Core.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    bool CheckIfNationalIdUnique(string nationalId);
    bool CheckIfEmailUnique(string email);
    bool CheckIfPhoneUnique(string phone);
    Task<User> FindUserAsyncById(int nationalId);
    Task<User?> FindUserAsyncByEmail(string email);
    Task<User?> FindUserAsyncByPhone(string phone);
    Task SaveChangesAsync();
}
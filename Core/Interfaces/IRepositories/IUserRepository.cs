using Core.Entities;

namespace Core.Interfaces.IRepositories;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    bool CheckIfNationalIdUnique(string nationalId);
    bool CheckIfEmailUnique(string email);
    bool CheckIfPhoneUnique(string phone);
    Task<User> FindUserAsyncById(Guid id);
    Task<User> FindUserAsyncByNationalId(int nationalId);
    Task<User?> FindUserAsyncByEmail(string email);
    Task<User?> FindUserAsyncByPhone(string phone);
    Task SaveChangesAsync();
    Task<(bool isSuccess,string errorMessage)> EnableNotificationsAsync(User user);
    Task<(bool isSuccess,string errorMessage)> DisableNotificationsAsync(User user);
}
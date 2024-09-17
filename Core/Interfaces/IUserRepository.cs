using Core.Entities;

namespace Core.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task AddUserContactInfoAsync(UserContactInfo userContactInfo);
    Task UpdateUserContactInfoAsync(UserContactInfo userContactInfo);
    public Task SaveChangesAsync();
}
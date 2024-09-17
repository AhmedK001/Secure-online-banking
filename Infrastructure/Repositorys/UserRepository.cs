using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositoryies;


public class UserRepository : IUserRepository
{

    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IUserContactInfo _userContactInfo;

    public UserRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public UserRepository(IUserContactInfo userContactInfo)
    {
        _userContactInfo = userContactInfo;
    }

    public async Task AddUserAsync(User user)
    {
        await _applicationDbContext.Users.AddAsync(user);
        await SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task AddUserContactInfoAsync(UserContactInfo userContactInfo)
    {
        await _applicationDbContext.ContactInfos.AddAsync(userContactInfo);
    }

    public Task UpdateUserContactInfoAsync(UserContactInfo userContactInfo)
    {
        throw new NotImplementedException();
    }

    public async Task SaveChangesAsync()
    {
        await _applicationDbContext.SaveChangesAsync();
        // make id registred manulally
    }
}
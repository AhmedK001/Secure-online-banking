using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositorys;


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

    public bool CheckIfNationalIdUnique(string nationalId)
    {
        if (long.TryParse(nationalId,out long nationalIdAsInt))
        {
            var isNationalIdUsed = _applicationDbContext.ContactInfos.Any(user =>
                user.NationalId == nationalIdAsInt);
            return !isNationalIdUsed;

        }

        Console.WriteLine("Couldn't parse");
        return false;
    }

    public bool CheckIfEmailUnique(string email)
    {
        var isEmailUsed = _applicationDbContext.ContactInfos.Any(user =>
            user.Email.Equals(email));

        return !isEmailUsed;
    }

    public bool CheckIfPhoneUnique(string phoneNumber)
    {
        var isPhoneNumberUsed
            = _applicationDbContext.ContactInfos.Any(user =>
                user.PhoneNumber.Equals(phoneNumber));

        return !isPhoneNumberUsed;
    }

    public async Task<User> FindUserAsyncById(int nationalId)
    {
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.NationalId == nationalId);
        return user;
    }
    
    public async Task<User?> FindUserAsyncByEmail(string email)
    {
        var userResult = await (from user in _applicationDbContext.Users
            join contactInfo in _applicationDbContext.ContactInfos 
                on user.NationalId equals contactInfo.NationalId
            where contactInfo.Email.Equals(email)
            select user).FirstOrDefaultAsync();

        return userResult;
    }

    public async Task<User?> FindUserAsyncByPhone(string phone)
    {
        var userResult = await (from user in _applicationDbContext.Users
            join contactInfo in _applicationDbContext.ContactInfos 
                on user.NationalId equals contactInfo.NationalId
            where contactInfo.PhoneNumber.Equals(phone)
            select user).FirstOrDefaultAsync();

        return userResult;
    }


    public async Task<UserContactInfo?> FindUserContactInfoById(int nationalId)
    {
        var userContactInfoResult
            = await _applicationDbContext.ContactInfos.FirstOrDefaultAsync(user =>
                user.NationalId.Equals(nationalId));

        return userContactInfoResult;
    }

    public async Task<UserContactInfo?> FindUserContactInfoByEmail(string email)
    {
        var userContactInfoResult
            = await _applicationDbContext.ContactInfos.FirstOrDefaultAsync(user =>
                user.Email.Equals(email));

        return userContactInfoResult;
    }

    public async Task<UserContactInfo?> FindUserContactInfoByPhone(string phoneNumber)
    {
        var userContactInfoResult
            = await _applicationDbContext.ContactInfos.FirstOrDefaultAsync(user =>
                user.PhoneNumber.Equals(phoneNumber));

        return userContactInfoResult;
    }

    public async Task SaveChangesAsync()
    {
        await _applicationDbContext.SaveChangesAsync();
    }
}
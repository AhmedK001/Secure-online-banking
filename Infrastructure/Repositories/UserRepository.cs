﻿using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.IRepositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;


public class UserRepository : IUserRepository
{

    private readonly ApplicationDbContext _applicationDbContext;

    public UserRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }


    public async Task AddUserAsync(User user)
    {
        await _applicationDbContext.Users.AddAsync(user);
    }

    public async Task UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public bool CheckIfNationalIdUnique(string nationalId)
    {
        if (long.TryParse(nationalId, out long nationalIdAsInt))
        {
            var isNationalIdUsed = _applicationDbContext.Users.Any(user =>
                user.NationalId == nationalIdAsInt);
            return !isNationalIdUsed;

        }

        Console.WriteLine("Couldn't parse");
        return false;
    }

    public bool CheckIfEmailUnique(string email)
    {
        var isEmailUsed = _applicationDbContext.Users.Any(user =>
            user.Email.Equals(email));

        return !isEmailUsed;
    }

    public bool CheckIfPhoneUnique(string phoneNumber)
    {
        var isPhoneNumberUsed
            = _applicationDbContext.Users.Any(user =>
                user.PhoneNumber.Equals(phoneNumber));

        return !isPhoneNumberUsed;
    }

    public async Task<User> FindUserAsyncById(Guid id)
    {
        var user
            = await _applicationDbContext.Users.FirstOrDefaultAsync(u =>
                u.Id == id);
        return user;
    }

    public async Task<User> FindUserAsyncByNationalId(int nationalId)
    {
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.NationalId == nationalId);
        return user;
    }

    public async Task<User?> FindUserAsyncByEmail(string email)
    {
        var userResult = await _applicationDbContext.Users
            .Where(user => user.Email.Equals(email))
            .FirstOrDefaultAsync();

        return userResult;
    }

    public async Task<User?> FindUserAsyncByPhone(string phone)
    {
        var userResult = await _applicationDbContext.Users
            .Where(user => user.PhoneNumber.Equals(phone))
            .FirstOrDefaultAsync();

        return userResult;
    }


    public async Task SaveChangesAsync()
    {
        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<(bool isSuccess, string errorMessage)> DisableNotificationsAsync(User user)
    {
        try
        {
            if (!user.Notifications)
            {
                return (false, "Notifications are disabled already.");
            }
            user.Notifications = false;
            await _applicationDbContext.SaveChangesAsync();
            return (true, "");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<(bool isSuccess, string errorMessage)> EnableNotificationsAsync(User user)
    {
        try
        {
            if (user.Notifications)
            {
                return (false, "Notifications are enabled already.");
            }
            user.Notifications = true;
            await _applicationDbContext.SaveChangesAsync();
            return (true, "");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


}
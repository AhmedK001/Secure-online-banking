using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly ApplicationDbContext _dbContext;


    public BankAccountRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsUserHasBankAccount(Guid id)
    {
        var isAccountExists
            = await _dbContext.Accounts.AnyAsync(a => a.UserId == id);

        return isAccountExists;
    }

    public async Task<BankAccount> CreateBankAccount(BankAccount bankAccount)
    {
        var addBankAccountResult
            = await _dbContext.Accounts.AddAsync(bankAccount);

        await _dbContext.SaveChangesAsync();

        return addBankAccountResult.Entity;
    }

    public async Task<BankAccount> GetBankAccountDetailsById(Guid id)
    {
        var accountDetailsResult
            = await _dbContext.Accounts.FirstOrDefaultAsync(a =>
                a.UserId == id);

        if (accountDetailsResult is null)
        {
            throw new KeyNotFoundException("Bank account not found.");
        }

        return accountDetailsResult;
    }

    public async Task<BankAccount> GetBankAccountDetailsByNationalId(int nationalId)
    {
        var accountDetailsResult
            = await _dbContext.Accounts.FirstOrDefaultAsync(a =>
                a.NationalId == nationalId);

        if (accountDetailsResult is null)
        {
            throw new KeyNotFoundException("Bank account not found.");
        }

        return accountDetailsResult;
    }

    public async Task<BankAccount> GetBankAccountDetailsByAccountNumber(
        string accountNumber)
    {
        var accountDetailsResult
            = await _dbContext.Accounts.FirstOrDefaultAsync(a =>
                a.AccountNumber == accountNumber);

        if (accountDetailsResult is null)
        {
            throw new KeyNotFoundException("Bank account not found.");
        }

        return accountDetailsResult;
    }

    public async Task<Decimal> GetBankAccountBalance(int nationalId)
    {
        var accountBalanceResult
            = await _dbContext.Accounts.FirstOrDefaultAsync(a =>
                a.NationalId == nationalId);

        if (accountBalanceResult is null)
        {
            throw new KeyNotFoundException("Bank account balance not found.");
        }

        return accountBalanceResult.Balance;
    }

    public async Task<bool> ChargeAccount(Guid id,decimal amount)
    {
        try
        {
            var chargeRequestResult = await _dbContext.Accounts.FirstAsync(a => a.UserId == id);

            if (chargeRequestResult == null)
            {
                return false;
            }
            chargeRequestResult.Balance = chargeRequestResult.Balance + amount;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<bool> DeductAccountBalance(string accountNumber, decimal amount)
    {
        try
        {
            var bankAccount = await _dbContext.Accounts.FirstAsync(a => a.AccountNumber == accountNumber);

            bankAccount.Balance -= amount;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<bool> ChangeCurrencyAsync(EnumCurrency currency, string accountNumber)
    {
        try
        {
            var bankAccountDetails = await GetBankAccountDetailsByAccountNumber(accountNumber);
            bankAccountDetails.Currency = currency;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<(bool isSuccess, decimal amountAfterExchange)> ChangeBalance(decimal newBalance, string accountNumber)
    {
        try
        {
            var bankAccountDetails = await _dbContext.Accounts.FirstAsync(a => a.AccountNumber == accountNumber);

            if (bankAccountDetails.Balance == 0)
            {
                throw new Exception("No balance to exchange.");
            }

            bankAccountDetails.Balance = newBalance;

            await _dbContext.SaveChangesAsync();
            return (true, bankAccountDetails.Balance);
        }
            
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
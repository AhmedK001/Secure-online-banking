﻿using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Core.Interfaces.IRepositories;

public interface IBankAccountRepository
{
    Task<bool> IsUserHasBankAccount(Guid id);
    Task<BankAccount> CreateBankAccount(BankAccount bankAccount);
    Task<BankAccount> GetBankAccountDetailsById(Guid id);
    Task<BankAccount> GetBankAccountDetailsByNationalId(int nationalId);
    Task<BankAccount> GetBankAccountDetailsByAccountNumber(string accountNumber);
    Task<decimal> GetBankAccountBalance(int nationalId);
    Task<bool> ChargeAccount(Guid id, decimal amount);
    Task<bool> DeductAccountBalance(string accountNumber, decimal amount);
    Task<bool> ChangeCurrencyAsync(bool saveAsync,EnumCurrency currency, string accountNumber);
    Task<(bool isSuccess, decimal amountAfterExchange )> ChangeBalance(bool saveAsync,decimal newBalance,
        string accountNumber);
}
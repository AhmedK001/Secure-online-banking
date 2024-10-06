using Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Core.Interfaces.IRepositories;

public interface IBankAccountRepository
{
    Task<bool> IsUserHasBankAccount(Guid id);
    Task<BankAccount> CreateBankAccount(BankAccount bankAccount);
    Task<BankAccount> GetBankAccountDetailsByNationalId(Guid id);
    Task<BankAccount> GetBankAccountDetailsByNationalId(int nationalId);
    Task<BankAccount> GetBankAccountDetailsByAccountNumber(string accountNumber);
    Task<decimal> GetBankAccountBalance(int nationalId);

}
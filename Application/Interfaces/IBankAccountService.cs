using Core.Entities;
using Core.Enums;

namespace Application.Interfaces;

public interface IBankAccountService : IIbanGeneratorService
{
    Task<bool> IsUserHasBankAccount(Guid id);
    Task<BankAccount> CreateBankAccount(Guid id);
    BankAccount GenerateDetails(User user);
    Task<bool> ChargeAccount(Guid id, decimal amount,BankAccount bankAccount);
    Task<BankAccount> GetDetailsById(Guid id);
    Task<BankAccount> GetDetailsByNationalId(int nationalId);
    Task<BankAccount> GetDetailsByAccountNumber(string accountNumber);
    Task<decimal> GetBalance(int nationalId);
    Task<bool> DeductAccountBalance(string accountNumber, decimal amount);
    Task<bool> ChangeCurrencyAsync(EnumCurrency currency, string accountNumber);
}
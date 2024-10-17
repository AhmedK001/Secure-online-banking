using Core.Entities;

namespace Application.Interfaces;

public interface IBankAccountService : IIbanGeneratorService
{
    Task<bool> IsUserHasBankAccount(Guid id);
    Task<BankAccount> CreateBankAccount(Guid id);
    BankAccount GenerateBankAccountDetails(User user);
    Task<bool> ChargeAccount(Guid id, decimal amount,BankAccount bankAccount);
    Task<BankAccount> GetBankAccountDetailsById(Guid id);
    Task<BankAccount> GetBankAccountDetailsByNationalId(int nationalId);
    Task<BankAccount> GetBankAccountDetailsByAccountNumber(string accountNumber);
    Task<decimal> GetBankAccountBalance(int nationalId);

}
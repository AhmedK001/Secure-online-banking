using Core.Entities;

namespace Application.Interfaces;

public interface IBankAccountService : IIbanGeneratorService
{
    Task<bool> IsUserHasBankAccount(Guid id);
    Task<BankAccount> CreateBankAccount(Guid id);
    BankAccount GenerateBankAccountDetails(User user);
}
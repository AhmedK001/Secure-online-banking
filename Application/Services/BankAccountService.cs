using Application.Interfaces;
using Core.Entities;
using Core.Interfaces.IRepositories;

namespace Application.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUserRepository _userRepository;

    public BankAccountService(IBankAccountRepository bankAccountRepository, IUserRepository userRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _userRepository = userRepository;
    }


    public string GenerateIban(int nationalId)
    {

        string nationalIdAsString = nationalId.ToString();

        string ibanPrefix = "00";


        Random random = new Random();

        // generate random 7 digits, In order to make iban numbers hardly tracked.
        int random7Digits = GeneratedRandom7DigitsNumber();

        // first 4 digits of the national Id
        string nationalIdFirst4Digits = nationalIdAsString.Substring(0,4);
        // final 4 digits of the national Id
        string nationalIdFianl4Digits = nationalIdAsString.Substring(nationalIdAsString.Length - 4,nationalIdAsString.Length -6);

        // Form of generating Iban
        return
            $"{ibanPrefix}{nationalIdFirst4Digits}0{random7Digits}0{nationalIdFianl4Digits}";
    }

    public async Task<bool> IsUserHasBankAccount(Guid id)
    {
        return await _bankAccountRepository.IsUserHasBankAccount(id);
    }

    public async Task<BankAccount> CreateBankAccount(Guid id)
    {
        var user = await _userRepository.FindUserAsyncById(id);

        if (user is null)
        {
            throw new KeyNotFoundException();
        }

        var bankAccountDetails = GenerateBankAccountDetails(user);

        var result
            = _bankAccountRepository.CreateBankAccount(bankAccountDetails);

        return await result;
    }

    public BankAccount GenerateBankAccountDetails(User user)
    {
        BankAccount finalBankAccountDetails = new BankAccount()
        {
            AccountNumber = GenerateIban(user.NationalId), // generate account number
            UserId = user.Id,
            NationalId = user.NationalId,
            Balance = 0,
            CreationDate = DateTime.UtcNow,
        };

        return finalBankAccountDetails;
    }

    public async Task<bool> ChargeAccount(Guid id, decimal amount)
    {
        var chargeResult = await _bankAccountRepository.ChargeAccount(id, amount);
        if (chargeResult == false)
        {
            return false;
        }
        return true;

    }

    public async Task<BankAccount> GetBankAccountDetailsById(Guid id)
    {
        var getDeatilsResult
            = await _bankAccountRepository.GetBankAccountDetailsById(id);

        return getDeatilsResult;
    }
    public Task<BankAccount> GetBankAccountDetailsByNationalId(int nationalId)
    {
        throw new NotImplementedException();
    }

    public Task<BankAccount> GetBankAccountDetailsByAccountNumber(string accountNumber)
    {
        throw new NotImplementedException();
    }

    public Task<decimal> GetBankAccountBalance(int nationalId)
    {
        throw new NotImplementedException();
    }

    private int GeneratedRandom7DigitsNumber()
    {
        Random random = new Random();

        // generate random 7 digits, In order to make iban numbers hardly tracked.
        return random.Next(2090209, 8607080);
    }
}
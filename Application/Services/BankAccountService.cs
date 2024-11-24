using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOperationService _operationService;
    private readonly IOperationsRepository _operationsRepository;
    private readonly ICurrencyService _currencyService;

    public BankAccountService(IOperationsRepository operationsRepository,IBankAccountRepository bankAccountRepository, IUserRepository userRepository, IOperationService operationService,ICurrencyService currencyService)
    {
        _bankAccountRepository = bankAccountRepository;
        _userRepository = userRepository;
        _operationService = operationService;
        _operationsRepository = operationsRepository;
        _currencyService = currencyService;
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

        var bankAccountDetails = GenerateDetails(user);

        var result
            = _bankAccountRepository.CreateBankAccount(bankAccountDetails);

        return await result;
    }

    public BankAccount GenerateDetails(User user)
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

    public async Task<bool> ChargeAccount(Guid id, decimal amount,BankAccount bankAccount)
    {
        var chargeResult = await _bankAccountRepository.ChargeAccount(id, amount);
        if (chargeResult == false)
        {
            return false;
        }

        // create Operation object
        Operation operation = new Operation()
        {
            OperationId
                = await _operationService
                    .GenerateUniqueRandomOperationIdAsync(),
            AccountNumber = bankAccount.AccountNumber,
            Amount = amount,
            AccountId = bankAccount.NationalId,
            OperationType = EnumOperationType.Deposit,
            DateTime = DateTime.UtcNow,
            Receiver = null,
            Description = null
        };

        await _operationService.AddOperation(operation);
        return true;
    }

    public async Task<BankAccount> GetDetailsById(Guid id)
    {
        var getDeatilsResult
            = await _bankAccountRepository.GetBankAccountDetailsById(id);

        return getDeatilsResult;
    }
    public Task<BankAccount> GetDetailsByNationalId(int nationalId)
    {
        throw new NotImplementedException();
    }

    public async Task<BankAccount> GetDetailsByAccountNumber(string accountNumber)
    {
        try
        {
            var getDeatilsResult = await _bankAccountRepository.GetBankAccountDetailsByAccountNumber(accountNumber);

            return getDeatilsResult;
        }
        catch (Exception e)
        {
            throw new Exception("",e);
        }
    }

    public Task<decimal> GetBalance(int nationalId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeductAccountBalance(string accountNumber, decimal amount)
    {
        try
        {
            await _bankAccountRepository.DeductAccountBalance(accountNumber, amount);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> ChangeCurrencyAsync(EnumCurrency currency, string accountNumber)
    {
        try
        {
            var bankAccountDetails
                = await _bankAccountRepository.GetBankAccountDetailsByAccountNumber(accountNumber);
            if (bankAccountDetails.Currency == currency)
            {
                throw new InvalidOperationException("The bank account already uses this currency.");
            }

            if (bankAccountDetails.Balance == 0)
            {
                await _bankAccountRepository.ChangeCurrencyAsync(currency, accountNumber);
                return true;
            }

            await ExchangeMoney(bankAccountDetails.Currency, currency, accountNumber);
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<bool> ExchangeMoney(EnumCurrency fromCurrency, EnumCurrency toCurrency, string accountNumber)
    {
        try
        {
            // make sure of null
            var exchangeForm = await _currencyService.GetExchangeForm(fromCurrency, toCurrency);
            var bankAccountDetails = await _bankAccountRepository.GetBankAccountDetailsByAccountNumber(accountNumber);
            if (bankAccountDetails.Currency != fromCurrency)
            {
                throw new Exception("You already uses the same currency.");
            }

            await _bankAccountRepository.ChangeCurrencyAsync(toCurrency, accountNumber);
            var amountAfterExchange = (decimal.Parse(exchangeForm.BidPrice) * bankAccountDetails.Balance);
            var result = await _bankAccountRepository.ChangeBalance(amountAfterExchange, accountNumber);
            if (!result.isSuccess)
            {
                throw new Exception("Something went wrong.");
            }

            return true;
        }
        catch (Exception e)
        {
            throw new Exception("Something went wrong.",e);
        }
    }

    private int GeneratedRandom7DigitsNumber()
    {
        Random random = new Random();

        // generate random 7 digits, In order to make iban numbers hardly tracked.
        return random.Next(2090209, 8607080);
    }


}
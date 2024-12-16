using Application.DTOs;
using Application.DTOs.ExternalModels.Currency;
using Application.Interfaces;
using Application.Mappers;
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
    private readonly IUnitOfWork _unitOfWork;
    //private readonly ICardsService _cardsService;
    private readonly ICardRepository _cardRepository;

    public BankAccountService(ICardRepository cardRepository,IOperationsRepository operationsRepository,IBankAccountRepository bankAccountRepository, IUserRepository userRepository, IOperationService operationService,ICurrencyService currencyService,IUnitOfWork unitOfWork)
    {
        _bankAccountRepository = bankAccountRepository;
        _userRepository = userRepository;
        _operationService = operationService;
        _operationsRepository = operationsRepository;
        _currencyService = currencyService;
        _unitOfWork = unitOfWork;
        //_cardsService = cardsService;
        _cardRepository = cardRepository;
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
            throw new KeyNotFoundException("User not logged in.");
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
        //
        // // create Operation object
        // Operation operation = new Operation()
        // {
        //     AccountNumber = bankAccount.AccountNumber,
        //     AccountId = bankAccount.NationalId,
        //     OperationId
        //         = await _operationService
        //             .GenerateUniqueRandomOperationIdAsync(),
        //     OperationType = EnumOperationType.Deposit,
        //     Amount = amount,
        //     Currency = bankAccount.Currency,
        //     DateTime = DateTime.UtcNow,
        //     Description = null,
        //     Receiver = null!
        // };
        //
        // await _operationService.LogOperation(true,operation);
        return true;
    }

    public async Task<BankAccount> GetDetailsById(Guid id)
    {
        try
        {
            var getDeatilsResult
                = await _bankAccountRepository.GetBankAccountDetailsById(id);

            return getDeatilsResult;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
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
            throw new Exception(e.Message);
        }
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
            throw new Exception(e.Message);
        }
    }

    public async Task<bool> ChangeCurrencyAsync(EnumCurrency currency, string accountNumber)
    {
        try
        {
            bool zeroBalance = false;
            var bankAccountDetails
                = await _bankAccountRepository.GetBankAccountDetailsByAccountNumber(accountNumber);
            if (bankAccountDetails.Currency == currency)
            {
                throw new InvalidOperationException("The bank account already uses this currency.");
            }

            if (bankAccountDetails.Balance == 0) zeroBalance = true;

            await ExchangeMoney(zeroBalance,Enum.GetName(typeof(EnumCurrency),bankAccountDetails.Currency), Enum.GetName(typeof(EnumCurrency),currency), accountNumber);

            return true;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<bool> ExchangeMoney(bool zeroBalance,string fromCurrency, string toCurrency, string accountNumber)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            ExchangeRateDto exchangeForm = null;
            decimal amountAfterExchange;
            var bankAccountDetails = await _bankAccountRepository.GetBankAccountDetailsByAccountNumber(accountNumber);
            var balanceBeforeExchange = bankAccountDetails.Balance;
            var oldCurrency = bankAccountDetails.Currency;

            if (!zeroBalance)
            {
                exchangeForm = await _currencyService.GetExchangeForm(fromCurrency, toCurrency);
                amountAfterExchange = (decimal.Parse(exchangeForm.BidPrice) * bankAccountDetails.Balance);
            }
            else
            {
                amountAfterExchange = 0;
            }
            if (Enum.GetName(typeof(EnumCurrency), bankAccountDetails.Currency) == toCurrency)
            {
                throw new Exception("You already uses the same currency.");
            }

            EnumCurrency.TryParse(toCurrency,out EnumCurrency currency);


            Operation operation = new Operation()
            {
                AccountNumber = bankAccountDetails.AccountNumber,
                AccountId = bankAccountDetails.NationalId,
                OperationId
                    = await _operationService
                        .GenerateUniqueRandomOperationIdAsync(),
                OperationType = EnumOperationType.CurrencyChange,
                Description
                    = $"Account currency change. " +
                      $" Balance before and after exchange: {Global.FormatCurrency(oldCurrency,balanceBeforeExchange)}, {Global.FormatCurrency(currency,amountAfterExchange)}" ,
                DateTime = DateTime.UtcNow,
                Currency = currency,
                Amount = bankAccountDetails.Balance,
            };

            await _operationService.AddOperation(true,operation);
            await _bankAccountRepository.ChangeCurrencyAsync(true,currency, accountNumber);
            if (!zeroBalance)
            {
                await _bankAccountRepository.ChangeBalance(true,amountAfterExchange, accountNumber);
            }
            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new Exception();
        }
    }

    public async Task<(bool isSuccess, decimal amountAfterExchange)> ChangeBalance(bool saveAsync, decimal newBalance, string accountNumber)
    {
        try
        {
            return await _bankAccountRepository.ChangeBalance(saveAsync, newBalance, accountNumber);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<(bool, string)> BankWithCardExchange(bool isBankToCard,ExchangeMoneyDtoBankAndCard exchangeDto, Card card, BankAccount bankAccount)
    {
        try
        {
            if (bankAccount.AccountNumber != card.BankAccount.AccountNumber)
                return (false, "This service only valid for internal transactions");

            if (isBankToCard)
            {

                var exchangeForm = await _currencyService.GetExchangeForm(Enum.GetName(typeof(EnumCurrency), bankAccount.Currency),
                    Enum.GetName(typeof(EnumCurrency), card.Currency));

                var amountAfterExchange = exchangeDto.Amount * decimal.Parse(exchangeForm.BidPrice);

                await _unitOfWork.BeginTransactionAsync();
                // Exchange process
                await ChangeBalance(true, bankAccount.Balance -= exchangeDto.Amount,bankAccount.AccountNumber);
                await _cardRepository.ChangeBalance(true, card.Balance += amountAfterExchange, card.CardId);
                await _unitOfWork.CommitTransactionAsync();

                return (true, "");
            }
            else
            {
                var exchangeForm = await _currencyService.GetExchangeForm(Enum.GetName(typeof(EnumCurrency), bankAccount.Currency),
                    Enum.GetName(typeof(EnumCurrency), card.Currency));

                var amountAfterExchange = exchangeDto.Amount * decimal.Parse(exchangeForm.BidPrice);

                await _unitOfWork.BeginTransactionAsync();
                // Exchange process
                await _cardRepository.ChangeBalance(true, card.Balance -= exchangeDto.Amount,card.CardId);
                await ChangeBalance(true, bankAccount.Balance += amountAfterExchange, bankAccount.AccountNumber);
                await _unitOfWork.CommitTransactionAsync();

                return (true, "");
            }
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private int GeneratedRandom7DigitsNumber()
    {
        Random random = new Random();

        // generate random 7 digits, In order to make iban numbers hardly tracked.
        return random.Next(2090209, 8607080);
    }
}
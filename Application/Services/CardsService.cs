using Application.DTOs.ExternalModels.Currency;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;

namespace Application.Services;

public class CardsService : ICardsService
{
    private readonly ICardRepository _cardRepository;
    private readonly IGenerateService _generateService;
    private readonly ICurrencyService _currencyService;
    private readonly IOperationService _operationService;
    private readonly IUnitOfWork _unitOfWork;

    public CardsService(ICardRepository cardRepository, IGenerateService generateService,
        ICurrencyService currencyService, IOperationService operationService, IUnitOfWork unitOfWork)
    {
        _cardRepository = cardRepository;
        _generateService = generateService;
        _currencyService = currencyService;
        _operationService = operationService;
        _unitOfWork = unitOfWork;
    }

    public Task<bool> IsUserHasCardWithInTypeAsync(string accountNumber, EnumCardType cardType)
    {
        throw new NotImplementedException();
    }

    public async Task<(bool, int)> CreateCardAsync(string accountNumber, string cardType, BankAccount bankAccount)
    {
        if (!EnumCardType.TryParse(cardType, out EnumCardType enumCardType))
        {
            string cardTypes = string.Join(", ", Enum.GetNames(typeof(EnumCardType)));
            throw new ArgumentException($"Card type not available on our system. Available types are: {cardTypes}");
        }

        if (await _cardRepository.HasThreeCards(accountNumber))
        {
            throw new Exception("You has the maximum allowed number of Cards.");
        }

        Card card = new Card()
        {
            Balance = 0,
            IsActivated = true,
            CardType = enumCardType,
            ExpiryDate = DateTime.UtcNow.AddYears(5),
            OpenedForInternalOperations = true,
            OpenedForOnlinePurchase = false,
            Payments = new List<Payment>(),
            BankAccount = bankAccount,
            Cvv = _generateService.GenerateRandomNumbers(Tuple.Create(100, 999)),
            CardId = await GenerateCardId()
        };

        try
        {
            await _cardRepository.CreateCardAsync(card, bankAccount);
            return (true, card.CardId);
        }
        catch (Exception e)
        {
            throw new Exception("Exception: " + e.StackTrace);
        }
    }

    public async Task<Card> GetCardDetails(string accountNumber, int cardId)
    {
        try
        {
            return await _cardRepository.GetCardDetails(accountNumber, cardId);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<Card>> GetAllCards(string accountNumber)
    {
        try
        {
            var result = await _cardRepository.GetAllCards(accountNumber);
            return result;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<bool> ChargeCardBalanceAsync(string accountNumber, int cardId, decimal amount)
    {
        try
        {
            await _cardRepository.ChargeCardBalanceAsync(accountNumber, cardId, amount);
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("Exception :", e);
        }
    }

    public Task<bool> DeductCardBalanceAsync(string accountNumber, int cardNumber, decimal amount)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsOpenedForOnlinePurchase(string accountNumber, int cardId)
    {
        try
        {
            var result = await _cardRepository.IsOpenedForOnlinePurchase(accountNumber, cardId);
            return result;
        }
        catch (Exception e)
        {
            throw new Exception("Error occurred: ", e);
        }
    }

    public async Task<bool> IsOpenedForInternalOperations(string accountNumber, int cardId)
    {
        try
        {
            var result = await _cardRepository.IsOpenedForInternalOperations(accountNumber, cardId);
            return result;
        }
        catch (Exception e)
        {
            throw new Exception("Error occurred: ", e);
        }
    }

    public async Task<bool> IsActivated(string accountNumber, int cardId)
    {
        try
        {
            var result = await _cardRepository.IsActivated(accountNumber, cardId);
            return result;
        }
        catch (Exception e)
        {
            throw new Exception("Error occurred: ", e);
        }
    }

    public async Task<int> GenerateCardId()
    {
        int cardId;
        do
        {
            cardId = _generateService.GenerateRandomNumbers(Tuple.Create(10000000, 99999999));
        } while (await _cardRepository.IsCardIdUnique(cardId) != true);

        return cardId;
    }

    public async Task<bool> DeleteCard(int cardId)
    {
        try
        {
            await _cardRepository.DeleteCard(cardId);
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<bool> ChangeCurrencyAsync(EnumCurrency currency, int cardId, string accountNumber)
    {
        try
        {
            var cardDetails = await _cardRepository.GetCardDetails(accountNumber, cardId);

            if (cardDetails.Currency == currency)
            {
                throw new Exception("Your card already has the same currency.");
            }

            if (cardDetails.Balance == 0)
            {
                await ExchangeMoney(true, Enum.GetName(typeof(EnumCurrency), cardDetails.Currency),
                    Enum.GetName(typeof(EnumCurrency), currency), cardId, accountNumber);
            }
            else
            {
                await ExchangeMoney(false, Enum.GetName(typeof(EnumCurrency), cardDetails.Currency),
                    Enum.GetName(typeof(EnumCurrency), currency), cardId, accountNumber);
            }

            return true;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<bool> ExchangeMoney(bool zeroBalance, string fromCurrency, string toCurrency, int cardId,
        string accountNumber)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(); // start saving data process

            // Make operations get saved if balance = 0
            ExchangeRateDto exchangeForm = null;
            decimal amountAfterExchange;
            var cardDetails = await _cardRepository.GetCardDetails(accountNumber, cardId);
            var balanceBeforeExchange = cardDetails.Balance;
            var bankAccountDetails = cardDetails.BankAccount;

            if (!zeroBalance)
            {
                exchangeForm = await _currencyService.GetExchangeForm(fromCurrency, toCurrency);
                amountAfterExchange = (decimal.Parse(exchangeForm.BidPrice) * cardDetails.Balance);
            }
            else
            {
                amountAfterExchange = 0;
            }

            if (Enum.GetName(typeof(EnumCurrency), cardDetails.Currency) != fromCurrency)
            {
                throw new Exception("Your card already uses the same currency.");
            }

            EnumCurrency.TryParse(toCurrency, out EnumCurrency currency);


            // Create Operation LogObject9
            Operation operation = new Operation()
            {
                AccountNumber = bankAccountDetails.AccountNumber,
                AccountId = bankAccountDetails.NationalId,
                OperationId = await _operationService.GenerateUniqueRandomOperationIdAsync(),
                OperationType = EnumOperationType.CurrencyChange,
                Description = $"CARD Currency Change. " + $"From {fromCurrency} To {toCurrency}," +
                              $" Balance before exchange: {balanceBeforeExchange:F2}{fromCurrency}," +
                              $" Balance After exchange: {amountAfterExchange:F2}{toCurrency}.",
                DateTime = DateTime.UtcNow,
                Currency = currency,
                Amount = bankAccountDetails.Balance,
            };

            await _operationService.LogOperation(true, operation);
            await _cardRepository.ChangeCurrencyAsync(true, currency, cardId);
            await _cardRepository.ChangeBalance(true, amountAfterExchange, cardId);
            await _unitOfWork.CommitTransactionAsync(); // save data
            return true;
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync(); // if failed, roll back all commits
            throw new Exception("Something went wrong.", e);
        }
    }
}
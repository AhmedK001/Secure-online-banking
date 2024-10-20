using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;

namespace Application.Services;

public class CardsService : ICardsService
{
    private readonly ICardRepository _cardRepository;
    private readonly IGenerateService _generateService;

    public CardsService(ICardRepository cardRepository,
        IGenerateService generateService)
    {
        _cardRepository = cardRepository;
        _generateService = generateService;
    }

    public Task<bool> IsUserHasCardWithInTypeAsync(string accountNumber,
        EnumCardType cardType)
    {
        throw new NotImplementedException();
    }

    public async Task<(bool , int)> CreateCardAsync(string accountNumber,
        string cardType,BankAccount bankAccount)
    {
        if (!EnumCardType.TryParse(cardType, out EnumCardType enumCardType))
        {
            throw new ArgumentException(
                "Card type not available on our system");
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
            Cvv = _generateService.GenerateRandomNumbers(
                Tuple.Create(100, 999)),
            CardId = await GenerateCardId()
        };

        try
        {
            await _cardRepository.CreateCardAsync(card,bankAccount);
            return (true,card.CardId);
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
            throw new Exception("Error occurred: ",e);
        }
    }

    public async Task<List<Card>> GetAllCards(string accountNumber)
    {
        var result = await _cardRepository.GetAllCards(accountNumber);

        return result;
    }

    public Task<bool> ChargeCardBalanceAsync(string accountNumber,
        int cardNumber, decimal amount)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeductCardBalanceAsync(string accountNumber,
        int cardNumber, decimal amount)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsOpenedForOnlinePurchase(string accountNumber,
        int cardNumber)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsOpenedForInternalOperations(string accountNumber,
        int cardNumber)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsActivated(string accountNumber, int cardNumber)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GenerateCardId()
    {
        int cardId;
        do
        {
            cardId = _generateService.GenerateRandomNumbers(
                Tuple.Create(10000000, 99999999));
        } while (await _cardRepository.IsCardIdUnique(cardId) != true);

        return cardId;
    }
}
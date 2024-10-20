using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.IRepositories;

public interface ICardRepository
{
    Task<List<Card>> GetAllCards(string accountNumber);
    Task<bool> IsUserHasCardWithInTypeAsync(string accountNumber, EnumCardType cardType);
    Task<bool> CreateCardAsync(Card card,BankAccount bankAccount);
    Task<Card> GetCardDetails(string accountNumber, int cardNumber);
    Task<bool> UpdateCardBalanceAsync(string accountNumber, int cardNumber, decimal amount);
    Task<bool> IsOpenedForOnlinePurchase(string accountNumber, int cardNumber);
    Task<bool> IsOpenedForInternalOperations(string accountNumber,
        int cardNumber);
    Task<bool> IsActivated(string accountNumber, int cardNumber);
    Task<bool> IsCardIdUnique(int cardId);

}
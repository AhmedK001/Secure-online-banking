using Application.DTOs;
using Core.Entities;
using Core.Enums;

namespace Application.Interfaces;

public interface ICardsService
{
    Task<bool> IsUserHasCardWithInTypeAsync(string accountNumber, EnumCardType cardType);
    Task<(bool,int)> CreateCardAsync(string accountNumber, string cardType,BankAccount bankAccount);
    Task<Card> GetCardDetails(string accountNumber, int cardNumber);
    Task<List<Card>> GetAllCards(string accountNumber);
    Task<bool> ChargeCardBalanceAsync(string accountNumber, int cardId, decimal amount);
    Task<bool> DeductCardBalanceAsync(string accountNumber, int cardNumber,
        decimal amount);
    Task<bool> IsOpenedForOnlinePurchase(string accountNumber, int cardId);
    Task<bool> IsOpenedForInternalOperations(string accountNumber,
        int cardId);
    Task<bool> IsActivated(string accountNumber, int cardId);

    Task<int> GenerateCardId();
    Task<bool> DeleteCard(int cardId);
    Task<bool> ChangeCurrencyAsync(EnumCurrency currency, int cardId, string accountNumber);
    Task<(bool isSuccess, decimal amountAfterExchange)> ChangeBalance(bool saveAsync, decimal newBalance,
        int cardId);
    Task<bool> ExchangeMoney(bool zeroBalance,string fromCurrency, string toCurrency,
        int cardId,string accountNumber);

    Task<(bool, string)> TransferToBankAccount(InternalTransactionDto transactionDto, BankAccount bankAccount, Card card);
    Task<(bool, string)> CardToCardExchange(ExchangeMoneyDtoCardToCard dtoCardToCard, Card baseCard, Card targetCard);
}
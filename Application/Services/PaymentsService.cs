using Application.Interfaces;
using Core.Entities;

namespace Application.Services;

public class PaymentsService : IPaymentsService
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ICardsService _cardsService;
    public PaymentsService(IBankAccountService bankAccountService, ICardsService cardsService)
    {
        _bankAccountService = bankAccountService;
        _cardsService = cardsService;
    }
    public async Task<bool> IsValidTransactionToCard(decimal amount, string userId, BankAccount bankAccount, Card card)
    {
        if (!await _bankAccountService.IsUserHasBankAccount(Guid.Parse(userId)))
        {
            throw new Exception("You must create a Bank Account.");
        }

        if (card == null)
        {
            throw new Exception("No cards found under this ID number.");
        }

        if (bankAccount.Currency != card.Currency)
        {
            throw new Exception($"Currencies of Card and Bank Account does not match. Card currency :{card.Currency}, Bank Account currency :{bankAccount.Currency}");
        }

        if (!card.OpenedForInternalOperations || !card.IsActivated)
        {
            throw new Exception("You card is not activated for this operation.");
        }

        if (card.Currency != bankAccount.Currency)
        {
            throw new Exception("Card and Bank Account has different currencies.");
        }

        if (bankAccount.Balance < amount)
        {
            throw new Exception(
                $"No enough balance. Your Bank Account balance is: {bankAccount.Balance} {bankAccount.Currency}");
        }

        return true;
    }
}
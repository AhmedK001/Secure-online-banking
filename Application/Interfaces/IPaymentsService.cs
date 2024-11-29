using Core.Entities;

namespace Application.Interfaces;

public interface IPaymentsService
{
    Task<bool> IsValidTransactionToCard(decimal amount, string userId, BankAccount bankAccount, Card card);
}
using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface IPaymentsService
{
    Task<(bool isSuccess, string errorMessage)> IsValidTransactionToCard(decimal amount, string userId,
        BankAccount bankAccount, Card card);
    (bool isValid, string ErrorMessage) IsAccountBalanceReachLimit(BankAccount bankAccount);

    (bool isValid, string ErrorMessage) IsValidTransactionToAccount(BankAccount bankAccount, Card card,
        InternalTransactionDto transactionDto);

    Task<(bool isSuccess, string ErrorMessage)> MakeTransactionToBank(User user,BankAccount bankAccount, Card card,
        InternalTransactionDto transactionDto);
    Task<(bool isSuccess, string ErrorMessage)> MakeTransactionToCard(User user,BankAccount bankAccount, Card card,
        InternalTransactionDto transactionDto);
    (bool isValid, string ErrorMessage) IsValidCurrencyTypeToCharge(BankAccount bankAccount);
}
using Core.Entities;

namespace Application.Interfaces;

public interface IEmailBodyBuilder
{
    string ChangeCurrencyBank(string message, BankAccount account);
    string ChangeCurrencyCard(string message, Card card);
    string CardToCardExchange(string message, Card baseCard, Card targetCard, decimal amountExchanged);
    string BankToCardExchange(string message, BankAccount bankAccount, Card card, decimal amountExchanged);
    string CardToBankExchange(string message, BankAccount bankAccount, Card card, decimal amountExchanged);
    string ChargeAccount(string message, BankAccount bankAccount, decimal chargeAmount, string paymentStatus);
    string TransferToCard(string message, BankAccount bankAccount, Card card, decimal amountTransferred);
    string TransferToAccount(string message, BankAccount bankAccount, Card card, decimal amountTransferred);
}
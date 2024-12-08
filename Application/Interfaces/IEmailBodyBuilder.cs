using Core.Entities;

namespace Application.Interfaces;

public interface IEmailBodyBuilder
{
    string ChangeCurrencyBankHtmlResponse(string message, BankAccount account);
    string ChangeCurrencyCardHtmlResponse(string message, Card card);
    string CardToCardExchangeHtmlResponse(string message, Card baseCard, Card targetCard, decimal amountExchanged);
    string BankToCardExchangeHtmlResponse(string message, BankAccount bankAccount, Card card, decimal amountExchanged);
    string CardToBankExchangeHtmlResponse(string message, BankAccount bankAccount, Card card, decimal amountExchanged);
    string ChargeAccountHtmlResponse(string message, BankAccount bankAccount, decimal chargeAmount, string paymentStatus);
    string TransferToCardHtmlResponse(string message, BankAccount bankAccount, Card card, decimal amountTransferred);
    string TransferToAccountHtmlResponse(string message, BankAccount bankAccount, Card card, decimal amountTransferred);
    string BuyStockHtmlResponse(string message, string stockName, string stockSymbol, decimal currentPrice,
        int numberOfStocks, decimal totalPrice);
    string TwoFactorAuthHtmlResponse(string message, string userName, string verificationCode, int expirationMinutes);
    string EmailConfirmationHtmlResponse(string message, string userName, string confirmationLink);

}
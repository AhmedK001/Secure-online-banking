using Application.DTOs.ResponseDto;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;

namespace Application.Services;

public class EmailBodyBuilder : IEmailBodyBuilder
{
    string FormatCurrency(EnumCurrency currency, decimal balance)
    {
        string currencySymbol = currency switch
        {
            EnumCurrency.SAR => "SAR",
            EnumCurrency.EGP => "EGP",
            EnumCurrency.AED => "AED",
            EnumCurrency.USD => "$",
            EnumCurrency.EUR => "€",
            EnumCurrency.TRY => "₺",
            _ => currency.ToString(),
        };

        return $"{currencySymbol} {balance:F2}";
    }

    public string ChangeCurrencyBankHtmlResponse(string message, BankAccount account)
    {
        string htmlContent = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                h1 {{ color: green; }}
                table {{ border-collapse: collapse; width: 100%; }}
                td {{ padding: 8px; border: 1px solid #ddd; }}
                th {{ padding: 8px; text-align: left; background-color: #f2f2f2; }}
                footer {{ font-size: 0.8em; color: #888; }}
            </style>
        </head>
        <body>
            <h1>{message}</h1>
            <p>Your account details have been updated as follows:</p>
            <table>
                <tr>
                    <th>Account Number:</th>
                    <td>{account.AccountNumber}</td>
                </tr>
                <tr>
                    <th>Creation Date:</th>
                    <td>{account.CreationDate:dd-MM-yyyy}</td>
                </tr>
                <tr>
                    <th>New Currency:</th>
                    <td>{Enum.GetName(typeof(EnumCurrency), account.Currency)}</td>
                </tr>
                <tr>
                    <th>Balance:</th>
                    <td>{FormatCurrency(account.Currency, account.Balance)}</td>
                </tr>
            </table>

            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string ChangeCurrencyCardHtmlResponse(string message, Card card)
    {
        string htmlContent = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                h1 {{ color: green; }}
                table {{ border-collapse: collapse; width: 100%; }}
                td {{ padding: 8px; border: 1px solid #ddd; }}
                th {{ padding: 8px; text-align: left; background-color: #f2f2f2; }}
                footer {{ font-size: 0.8em; color: #888; }}
            </style>
        </head>
        <body>
            <h1>{message}</h1>
            <p>Your card details have been updated as follows:</p>
            <table>
                <tr>
                    <th>Card ID:</th>
                    <td>{card.CardId}</td>
                </tr>
                <tr>
                    <th>Card Type:</th>
                    <td>{card.CardType}</td>
                </tr>
                <tr>
                    <th>Is Activated:</th>
                    <td>{(card.IsActivated ? "Yes" : "No")}</td>
                </tr>
                <tr>
                    <th>Currency:</th>
                    <td>{Enum.GetName(typeof(EnumCurrency), card.Currency)}</td>
                </tr>
                <tr>
                    <th>Balance:</th>
                    <td>{FormatCurrency(card.Currency, card.Balance)}</td>
                </tr>
            </table>

            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string CardToCardExchangeHtmlResponse(string message, Card baseCard, Card targetCard,
        decimal amountExchanged)
    {
        string htmlContent = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                h1 {{ color: green; }}
                table {{ border-collapse: collapse; width: 100%; }}
                td, th {{ padding: 8px; border: 1px solid #ddd; }}
                th {{ background-color: #f2f2f2; text-align: left; }}
                footer {{ font-size: 0.8em; color: #888; }}
            </style>
        </head>
        <body>
            <h1>{message}</h1>
            <p>Details of your card-to-card exchange:</p>
            <table>
                <tr>
                    <th>Field</th>
                    <th>Base Card</th>
                    <th>Target Card</th>
                </tr>
                <tr>
                    <td><strong>Card ID</strong></td>
                    <td>{baseCard.CardId}</td>
                    <td>{targetCard.CardId}</td>
                </tr>
                <tr>
                    <td><strong>Card Type</strong></td>
                    <td>{Enum.GetName(typeof(EnumCardType), baseCard.CardType)}</td>
                    <td>{Enum.GetName(typeof(EnumCardType), targetCard.CardType)}</td>
                </tr>
                <tr>
                    <td><strong>Currency</strong></td>
                    <td>{Enum.GetName(typeof(EnumCurrency), baseCard.Currency)}</td>
                    <td>{Enum.GetName(typeof(EnumCurrency), targetCard.Currency)}</td>
                </tr>
                <tr>
                    <td><strong>Balance</strong></td>
                    <td>{FormatCurrency(baseCard.Currency, baseCard.Balance)}</td>
                    <td>{FormatCurrency(targetCard.Currency, targetCard.Balance)}</td>
                </tr>
            </table>
            <p><strong>Amount Exchanged:</strong> {FormatCurrency(baseCard.Currency, amountExchanged)}</p>

            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string BankToCardExchangeHtmlResponse(string message, BankAccount bankAccount, Card card,
        decimal amountExchanged)
    {
        string htmlContent = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                h1 {{ color: green; }}
                table {{ border-collapse: collapse; width: 100%; }}
                td, th {{ padding: 8px; border: 1px solid #ddd; }}
                th {{ background-color: #f2f2f2; text-align: left; }}
                footer {{ font-size: 0.8em; color: #888; }}
            </style>
        </head>
        <body>
            <h1>{message}</h1>
            <p>Details of your bank-to-card exchange:</p>
            <table>
                <tr>
                    <th>Field</th>
                    <th>Bank Account</th>
                    <th>Card</th>
                </tr>
                <tr>
                    <td><strong>Account/Card Number</strong></td>
                    <td>{bankAccount.AccountNumber}</td>
                    <td>{card.CardId}</td>
                </tr>
                <tr>
                    <td><strong>Currency</strong></td>
                    <td>{Enum.GetName(typeof(EnumCurrency), bankAccount.Currency)}</td>
                    <td>{Enum.GetName(typeof(EnumCurrency), card.Currency)}</td>
                </tr>
                <tr>
                    <td><strong>Balance After Transaction</strong></td>
                    <td>{FormatCurrency(bankAccount.Currency, bankAccount.Balance)}</td>
                    <td>{FormatCurrency(card.Currency, card.Balance)}</td>
                </tr>
            </table>
            <p><strong>Amount Exchanged:</strong> {FormatCurrency(bankAccount.Currency, amountExchanged)}</p>

            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string CardToBankExchangeHtmlResponse(string message, BankAccount bankAccount, Card card,
        decimal amountExchanged)
    {
        string htmlContent = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                h1 {{ color: green; }}
                table {{ border-collapse: collapse; width: 100%; }}
                td, th {{ padding: 8px; border: 1px solid #ddd; }}
                th {{ background-color: #f2f2f2; text-align: left; }}
                footer {{ font-size: 0.8em; color: #888; }}
            </style>
        </head>
        <body>
            <h1>{message}</h1>
            <p>Details of your card-to-bank exchange:</p>
            <table>
                <tr>
                    <th>Field</th>
                    <th>Bank Account</th>
                    <th>Card</th>
                </tr>
                <tr>
                    <td><strong>Account/Card Number</strong></td>
                    <td>{bankAccount.AccountNumber}</td>
                    <td>{card.CardId}</td>
                </tr>
                <tr>
                    <td><strong>Currency</strong></td>
                    <td>{Enum.GetName(typeof(EnumCurrency), bankAccount.Currency)}</td>
                    <td>{Enum.GetName(typeof(EnumCurrency), card.Currency)}</td>
                </tr>
                <tr>
                    <td><strong>Balance After Transaction</strong></td>
                    <td>{FormatCurrency(bankAccount.Currency, bankAccount.Balance)}</td>
                    <td>{FormatCurrency(card.Currency, card.Balance)}</td>
                </tr>
            </table>
            <p><strong>Amount Exchanged:</strong> {FormatCurrency(bankAccount.Currency, amountExchanged)}</p>

            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string ChargeAccountHtmlResponse(string message, BankAccount bankAccount, decimal chargeAmount,
        string paymentStatus)
    {
        string htmlContent = $@"
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; }}
            h1 {{ color: green; }}
            table {{ border-collapse: collapse; width: 100%; }}
            td, th {{ padding: 8px; border: 1px solid #ddd; }}
            th {{ background-color: #f2f2f2; text-align: left; }}
            footer {{ font-size: 0.8em; color: #888; }}
        </style>
    </head>
    <body>
        <h1>{message}</h1>
        <p>Details of your payment transaction:</p>
        <table>
            <tr>
                <th>Field</th>
                <th>Bank Account</th>
            </tr>
            <tr>
                <td><strong>Account Number</strong></td>
                <td>{bankAccount.AccountNumber}</td>
            </tr>
            <tr>
                <td><strong>Payment Status</strong></td>
                <td>{paymentStatus}</td>
            </tr>
            <tr>
                <td><strong>Currency</strong></td>
                <td>{Enum.GetName(typeof(EnumCurrency), bankAccount.Currency)}</td>
            </tr>
            <tr>
                <td><strong>Balance After Payment</strong></td>
                <td>{FormatCurrency(bankAccount.Currency, bankAccount.Balance)}</td>
            </tr>
        </table>
        <p><strong>Amount Charged:</strong> {FormatCurrency(bankAccount.Currency, chargeAmount)}</p>
        <p>If you have any questions, please feel free to contact our support team.</p>
        <footer>
            <p>Thank you for using Secure Online Banking.</p>
        </footer>
    </body>
    </html>";

        return htmlContent;
    }

    public string TransferToCardHtmlResponse(string message, BankAccount bankAccount, Card card,
        decimal amountTransferred)
    {
        string htmlContent = $@"
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; }}
            h1 {{ color: green; }}
            table {{ border-collapse: collapse; width: 100%; }}
            td, th {{ padding: 8px; border: 1px solid #ddd; }}
            th {{ background-color: #f2f2f2; text-align: left; }}
            footer {{ font-size: 0.8em; color: #888; }}
        </style>
    </head>
    <body>
        <h1>{message}</h1>
        <p>Details of your transaction from bank account to card:</p>
        <table>
            <tr>
                <th>Field</th>
                <th>Bank Account</th>
                <th>Card</th>
            </tr>
            <tr>
                <td><strong>Account/Card Number</strong></td>
                <td>{bankAccount.AccountNumber}</td>
                <td>{card.CardId}</td>
            </tr>
            <tr>
                <td><strong>Currency</strong></td>
                <td>{Enum.GetName(typeof(EnumCurrency), bankAccount.Currency)}</td>
                <td>{Enum.GetName(typeof(EnumCurrency), card.Currency)}</td>
            </tr>
            <tr>
                <td><strong>Balance After Transaction</strong></td>
                <td>{FormatCurrency(bankAccount.Currency, bankAccount.Balance)}</td>
                <td>{FormatCurrency(card.Currency, card.Balance)}</td>
            </tr>
        </table>
        <p><strong>Amount Transferred:</strong> {FormatCurrency(bankAccount.Currency, amountTransferred)}</p>
        <p>If you have any questions, please feel free to contact our support team.</p>
        <footer>
            <p>Thank you for using Secure Online Banking.</p>
        </footer>
    </body>
    </html>";

        return htmlContent;
    }

    public string TransferToAccountHtmlResponse(string message, BankAccount bankAccount, Card card,
        decimal amountTransferred)
    {
        string htmlContent = $@"
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; }}
            h1 {{ color: green; }}
            table {{ border-collapse: collapse; width: 100%; }}
            td, th {{ padding: 8px; border: 1px solid #ddd; }}
            th {{ background-color: #f2f2f2; text-align: left; }}
            footer {{ font-size: 0.8em; color: #888; }}
        </style>
    </head>
    <body>
        <h1>{message}</h1>
        <p>Details of your transaction from card to bank account:</p>
        <table>
            <tr>
                <th>Field</th>
                <th>Bank Account</th>
                <th>Card</th>
            </tr>
            <tr>
                <td><strong>Account/Card Number</strong></td>
                <td>{bankAccount.AccountNumber}</td>
                <td>{card.CardId}</td>
            </tr>
            <tr>
                <td><strong>Currency</strong></td>
                <td>{Enum.GetName(typeof(EnumCurrency), bankAccount.Currency)}</td>
                <td>{Enum.GetName(typeof(EnumCurrency), card.Currency)}</td>
            </tr>
            <tr>
                <td><strong>Balance After Transaction</strong></td>
                <td>{FormatCurrency(bankAccount.Currency, bankAccount.Balance)}</td>
                <td>{FormatCurrency(card.Currency, card.Balance)}</td>
            </tr>
        </table>
        <p><strong>Amount Transferred:</strong> {FormatCurrency(bankAccount.Currency, amountTransferred)}</p>
        <p>If you have any questions, please feel free to contact our support team.</p>
        <footer>
            <p>Thank you for using Secure Online Banking.</p>
        </footer>
    </body>
    </html>";

        return htmlContent;
    }

    public string BuyStockHtmlResponse(string message, string stockName, string stockSymbol, decimal currentPrice,
        int numberOfStocks, decimal totalPrice)
    {
        string htmlContent = $@"
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; }}
            h1 {{ color: green; }}
            table {{ border-collapse: collapse; width: 100%; }}
            td {{ padding: 8px; border: 1px solid #ddd; }}
            th {{ padding: 8px; text-align: left; background-color: #f2f2f2; }}
            footer {{ font-size: 0.8em; color: #888; }}
        </style>
    </head>
    <body>
        <h1>{message}</h1>
        <p>Your stock purchase has been completed successfully. Details are as follows:</p>
        <table>
            <tr>
                <th>Stock Name:</th>
                <td>{stockName}</td>
            </tr>
            <tr>
                <th>Stock Symbol:</th>
                <td>{stockSymbol}</td>
            </tr>
            <tr>
                <th>Stock Price (per share):</th>
                <td>{currentPrice:C}</td>
            </tr>
            <tr>
                <th>Number of Stocks Purchased:</th>
                <td>{numberOfStocks}</td>
            </tr>
            <tr>
                <th>Total Price:</th>
                <td>{totalPrice:C}</td>
            </tr>
        </table>

        <footer>
            <p>Thank you for using Secure Online Banking for your investments.</p>
        </footer>
    </body>
    </html>";

        return htmlContent;
    }

    public string TwoFactorAuthHtmlResponse(string message, string userName, string verificationCode,
        int expirationMinutes)
    {
        string htmlContent = $@"
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; }}
            h1 {{ color: blue; }}
            table {{ border-collapse: collapse; width: 100%; }}
            td {{ padding: 8px; border: 1px solid #ddd; }}
            th {{ padding: 8px; text-align: left; background-color: #f2f2f2; }}
            .code {{ font-size: 1.2em; color: red; font-weight: bold; }}
            footer {{ font-size: 0.8em; color: #888; }}
        </style>
    </head>
    <body>
        <h1>{message}</h1>
        <p>Dear {userName},</p>
        <p>Your 2FA verification code is:</p>
        <p class='code'>{verificationCode}</p>
        <p>Please use this code to complete your login. The code will expire in: {expirationMinutes} minutes</p>

        <footer>
            <p>Thank you for using Secure Online Banking.</p>
        </footer>
    </body>
    </html>";

        return htmlContent;
    }

    public string EmailConfirmationHtmlResponse(string message, string userName, string confirmationLink)
    {
        string htmlContent = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .email-container {{
            width: 100%;
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
        }}
        h1 {{
            color: #333333;
            font-size: 24px;
            text-align: center;
        }}
        p {{
            font-size: 16px;
            color: #666666;
            line-height: 1.6;
        }}
        .button {{
            display: inline-block;
            background-color: #4CAF50;
            color: white;
            padding: 12px 25px;
            font-size: 16px;
            font-weight: bold;
            text-align: center;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
            transition: background-color 0.3s ease;
        }}
        .button:hover {{
            background-color: #45a049;
        }}
        footer {{
            text-align: center;
            font-size: 12px;
            color: #888888;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <h1>{message}</h1>
        <p>Hello {userName},</p>
        <p>Thank you for registering with us. To complete your registration, please confirm your email address by clicking the button below:</p>
        <a href='{confirmationLink}' class='button'>Confirm Email</a>
        <p>If you did not register, please ignore this email.</p>
        <footer>
            <p>Secure Online Banking.</p>
        </footer>
    </div>
</body>
</html>
";
        return htmlContent;
    }
}
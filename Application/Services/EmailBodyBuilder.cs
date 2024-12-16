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
            EnumCurrency.SAR => "ريال",
            EnumCurrency.EGP => "E£",
            EnumCurrency.AED => "د.إ",
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

    public string BuyStockHtmlResponse(string message, string stockName, string stockSymbol,
        decimal currentPrice, int numberOfStocks, decimal totalPrice)
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
        var htmlContent = $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f9f9f9;
                margin: 0;
                padding: 0;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: 20px auto;
                background: #ffffff;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                text-align: center;
                border-bottom: 2px solid #007bff;
                margin-bottom: 20px;
                padding-bottom: 10px;
            }}
            .header h1 {{
                color: #007bff;
                font-size: 24px;
                margin: 0;
            }}
            .content p {{
                line-height: 1.6;
                margin: 10px 0;
            }}
            .code {{
                font-size: 1.5em;
                color: #d9534f;
                font-weight: bold;
                text-align: center;
                margin: 20px 0;
                padding: 10px;
                background-color: #f8d7da;
                border-radius: 4px;
                display: inline-block;
            }}
            .footer {{
                margin-top: 20px;
                font-size: 0.9em;
                color: #666;
                text-align: center;
                border-top: 1px solid #ddd;
                padding-top: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>{message}</h1>
            </div>
            <div class='content'>
                <p>Dear <strong>{userName}</strong>,</p>
                <p>Your Two-Factor Authentication (2FA) verification code is:</p>
                <p class='code'>{verificationCode}</p>
                <p>Please use this code to complete your login. The code will expire in <strong>{expirationMinutes} minutes</strong>.</p>
            </div>
            <div class='footer'>
                <p>Thank you for using Secure Online Banking.</p>
            </div>
        </div>
    </body>
    </html>";

        return htmlContent;
    }

    public string EmailConfirmationHtmlResponse(string message, string userName, string confirmationLink)
    {
        var htmlContent = $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f9f9f9;
                margin: 0;
                padding: 0;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: 20px auto;
                background: #ffffff;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                text-align: center;
                border-bottom: 2px solid #007bff;
                margin-bottom: 20px;
                padding-bottom: 10px;
            }}
            .header h1 {{
                color: #007bff;
                font-size: 24px;
                margin: 0;
            }}
            .content p {{
                line-height: 1.6;
                margin: 10px 0;
            }}
            .confirmation-link {{
                display: inline-block;
                margin: 20px 0;
                padding: 10px 20px;
                font-size: 1.1em;
                color: #fff;
                background-color: #007bff;
                text-decoration: none;
                border-radius: 4px;
            }}
            .confirmation-link:hover {{
                background-color: #0056b3;
            }}
            .plain-link {{
                word-break: break-word;
                color: #007bff;
                text-decoration: underline;
            }}
            .footer {{
                margin-top: 20px;
                font-size: 0.9em;
                color: #666;
                text-align: center;
                border-top: 1px solid #ddd;
                padding-top: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>{message}</h1>
            </div>
            <div class='content'>
                <p>Dear <strong>{userName}</strong>,</p>
                <p>Thank you for signing up! Please confirm your email address by clicking the link below:</p>
                <a href='{confirmationLink}' class='confirmation-link'>Confirm Email</a>
                <p>If you prefer, you can copy and paste the following link into your browser:</p>
                <p class='plain-link'>{confirmationLink}</p>
                <p>If you did not create this account, please ignore this email.</p>
            </div>
            <div class='footer'>
                <p>Thank you for using Secure Online Banking.</p>
            </div>
        </div>
    </body>
    </html>";

        return htmlContent;
    }

    public string PasswordResetHtmlResponse(string message, string userEmail, string resetLink)
    {
        var htmlContent = $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f9f9f9;
                margin: 0;
                padding: 0;
                color: #333;
            }}
            .container {{
                max-width: 600px;
                margin: 20px auto;
                background: #ffffff;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                text-align: center;
                border-bottom: 2px solid #007bff;
                margin-bottom: 20px;
                padding-bottom: 10px;
            }}
            .header h1 {{
                color: #007bff;
                font-size: 24px;
                margin: 0;
            }}
            .content p {{
                line-height: 1.6;
                margin: 10px 0;
            }}
            .reset-link {{
                display: inline-block;
                margin: 20px 0;
                padding: 10px 20px;
                font-size: 1.1em;
                color: #fff;
                background-color: #007bff;
                text-decoration: none;
                border-radius: 4px;
            }}
            .reset-link:hover {{
                background-color: #0056b3;
            }}
            .plain-link {{
                word-break: break-word;
                color: #007bff;
                text-decoration: underline;
            }}
            .footer {{
                margin-top: 20px;
                font-size: 0.9em;
                color: #666;
                text-align: center;
                border-top: 1px solid #ddd;
                padding-top: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>{message}</h1>
            </div>
            <div class='content'>
                <p>Dear <strong>{userEmail}</strong>,</p>
                <p>We received a request to reset your password. Please click the button below to reset it:</p>
                <a href='{resetLink}' class='reset-link'>Reset Password</a>
                <p>If you prefer, you can copy and paste the following link into your browser:</p>
                <p class='plain-link'>{resetLink}</p>
                <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
            </div>
            <div class='footer'>
                <p>Thank you for using Secure Online Banking.</p>
            </div>
        </div>
    </body>
    </html>";

        return htmlContent;
    }
}
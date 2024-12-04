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

    public string ChangeCurrencyBank(string message, BankAccount account)
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
            <p>If you have any questions, please feel free to contact our support team.</p>
            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string ChangeCurrencyCard(string message, Card card)
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
            <p>If you have any questions, please feel free to contact our support team.</p>
            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string CardToCardExchange(string message, Card baseCard, Card targetCard, decimal amountExchanged)
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
            <p>If you have any questions, please feel free to contact our support team.</p>
            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string BankToCardExchange(string message, BankAccount bankAccount, Card card, decimal amountExchanged)
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
            <p>If you have any questions, please feel free to contact our support team.</p>
            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string CardToBankExchange(string message, BankAccount bankAccount, Card card, decimal amountExchanged)
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
            <p>If you have any questions, please feel free to contact our support team.</p>
            <footer>
                <p>Thank you for using Secure Online Banking.</p>
            </footer>
        </body>
        </html>";

        return htmlContent;
    }

    public string ChargeAccount(string message, BankAccount bankAccount, decimal chargeAmount, string paymentStatus)
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

    public string TransferToCard(string message, BankAccount bankAccount, Card card, decimal amountTransferred)
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

    public string TransferToAccount(string message, BankAccount bankAccount, Card card, decimal amountTransferred)
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
}
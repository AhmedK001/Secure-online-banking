using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;

namespace Application.Services;

public class PaymentsService : IPaymentsService
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ICardsService _cardsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IOperationService _operationService;
    private readonly IEmailBodyBuilder _emailBodyBuilder;


    public PaymentsService(IBankAccountService bankAccountService, ICardsService cardsService,
        IUnitOfWork unitOfWork, IEmailService emailService, IOperationService operationService,
        IEmailBodyBuilder emailBodyBuilder)
    {
        _bankAccountService = bankAccountService;
        _cardsService = cardsService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _operationService = operationService;
        _emailBodyBuilder = emailBodyBuilder;
    }

    public async Task<(bool isSuccess, string errorMessage)> IsValidTransactionToCard(decimal amount, string userId, BankAccount bankAccount,
        Card card)
    {
        if (amount <= 0)
        {
            return (false, "Invalid amount.");
        }

        if (!await _bankAccountService.IsUserHasBankAccount(Guid.Parse(userId)))
        {
            return (false, "You must create a Bank Account.");
        }

        if (card == null)
        {
            return (false, "No cards found under this ID number.");
        }

        if (bankAccount.Currency != card.Currency)
        {
            return (false, "$\"Currencies of Card and Bank Account does not match. Card currency :{card.Currency}, Bank Account currency :{bankAccount.Currency}\"");
        }

        if (bankAccount.Balance <= 0)
            return (false, $"No enough balance, Your card balance is {card.Balance.ToString("F2")}{card.Currency}");

        if (!card.OpenedForInternalOperations || !card.IsActivated)
        {
            return (false, "You card is not activated for this operation.");
        }

        if (card.Currency != bankAccount.Currency)
        {
            return (false, "Card and Bank Account has different currencies.");
        }

        if (bankAccount.Balance < amount)
        {
            return (false, $"No enough balance. Your Bank Account balance is: {bankAccount.Balance} {bankAccount.Currency}");
        }

        return (true,"");
    }

    public (bool isValid, string ErrorMessage) IsAccountBalanceReachLimit(BankAccount bankAccount)
    {
        if (bankAccount.Currency == EnumCurrency.USD || bankAccount.Currency == EnumCurrency.EUR)
        {
            if (bankAccount.Balance > 4930000)
            {
                return (false, "Account reach its limit, Contact support.");
            }
        }

        if (bankAccount.Currency == EnumCurrency.AED || bankAccount.Currency == EnumCurrency.SAR)
        {
            if (bankAccount.Balance > 9930000)
            {
                return (false, "Account reach its limit, Contact support.");
            }
        }

        if (bankAccount.Currency == EnumCurrency.EGP || bankAccount.Currency == EnumCurrency.TRY)
        {
            if (bankAccount.Balance > 19930000)
            {
                return (false, "Account reach its limit, Contact support.");
            }
        }

        return (true, "");
    }

    public (bool isValid, string ErrorMessage) IsValidTransactionToAccount(BankAccount bankAccount, Card card,
        InternalTransactionDto transactionDto)
    {
        if (transactionDto.Amount <= 0)
        {
            return (false, "Invalid amount.");
        }
        if (card.Currency != bankAccount.Currency)
            return (false, "Account and card currency does not match.");

        if (card.Balance <= 0)
            return (false,
                $"No enough balance, Your card balance is {card.Balance.ToString("F2")}{card.Currency}");

        if (card.Balance < transactionDto.Amount)
            return (false,
                $"No enough balance, Your card balance is {card.Balance.ToString("F2")}{card.Currency}");

        return (true, "");
    }

    public async Task<(bool isSuccess, string ErrorMessage)> MakeTransactionToBank(User user,
        BankAccount bankAccount, Card card, InternalTransactionDto transactionDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var isValidTransactionToAccount = IsValidTransactionToAccount(bankAccount, card, transactionDto);

            if (!isValidTransactionToAccount.isValid)
                return (false, isValidTransactionToAccount.ErrorMessage);

            var transactionResult
                = await _cardsService.TransferToBankAccount(transactionDto, bankAccount, card);

            if (!transactionResult.Item1)
                return (false, transactionResult.Item2);

            string emailContent = _emailBodyBuilder.TransferToAccountHtmlResponse(
                "Your transaction to the bank account has been completed successfully.", bankAccount, card,
                transactionDto.Amount);

            await _emailService.SendEmailAsync(user,
                "Your transaction to the bank account has been completed successfully.", emailContent);

            var operation = await _operationService.BuildTransferOperation(bankAccount, transactionDto.Amount,
                EnumOperationType.TransactionToAccount);
            await _operationService.ValidateAndSaveOperation(operation);

            await _unitOfWork.CommitTransactionAsync();

            return (true, "");
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<(bool isSuccess, string ErrorMessage)> MakeTransactionToCard(User user, BankAccount bankAccount, Card card,
        InternalTransactionDto cardDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            // this method will throw exception if not valid transaction
            var isValidTransaction
                = await IsValidTransactionToCard(cardDto.Amount, user.Id.ToString(), bankAccount, card);

            if (!isValidTransaction.isSuccess) return (false,isValidTransaction.errorMessage);

            await _bankAccountService.DeductAccountBalance(bankAccount.AccountNumber, cardDto.Amount);
            await _cardsService.ChargeCardBalanceAsync(bankAccount.AccountNumber, card.CardId,
                cardDto.Amount);

            // save operations
            var operation = await _operationService.BuildTransferOperation(bankAccount, cardDto.Amount,
                EnumOperationType.TransactionToCard);
            await _operationService.ValidateAndSaveOperation(operation);

            // send emails
            string emailContent = _emailBodyBuilder.TransferToCardHtmlResponse(
                "Your transaction to the card has been completed successfully.", bankAccount, card,
                cardDto.Amount);

            await _emailService.SendEmailAsync(user,
                "Your transaction to the card has been completed successfully.", emailContent);

            await _unitOfWork.CommitTransactionAsync();

            return (true, "");
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            Console.WriteLine(e);
            throw;
        }
    }

    public (bool isValid, string ErrorMessage) IsValidCurrencyTypeToCharge(BankAccount bankAccount)
    {
        var accountCurrency = Enum.GetName(typeof(EnumCurrency), bankAccount.Currency);
        if (accountCurrency != "EUR" && accountCurrency != "AED" && accountCurrency != "USD")
            return (false, "The bank account currency must be one of the following: AED, USD, or EUR.");

        return (true, "");
    }
}
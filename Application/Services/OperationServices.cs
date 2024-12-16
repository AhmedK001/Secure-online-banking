using Application.Interfaces;
using Application.Mappers;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;

namespace Application.Services;

public class OperationServices : IOperationService
{
    private readonly IOperationsRepository _operationsRepository;

    public OperationServices(IOperationsRepository operationsRepository)
    {
        _operationsRepository = operationsRepository;
    }

    public bool IsValidOperationObject(Operation? operation)
    {
        if (operation == null)
            return false;

        if (string.IsNullOrWhiteSpace(operation.AccountNumber))
            return false;

        if (operation.OperationId <= 0)
            return false;

        if (operation.DateTime == DateTime.MinValue)
            return false;

        if (operation.Amount <= 0)
            return false;

        return true;
    }

    public async Task<int> GenerateUniqueRandomOperationIdAsync()
    {
        Random random = new Random();
        int randomId;

        do
        {
            randomId = random.Next(10000000, 99999999); // Random number between 100000 and 999999
        } while (!await _operationsRepository.IsOperationIdUnique(randomId));

        return randomId; // Return unique random ID
    }

    public async Task AddOperation(bool saveAsync, Operation operation)
    {
        await _operationsRepository.AddOperation(saveAsync, operation);
    }

    public async Task<bool> ValidateAndSaveOperation(Operation operation)
    {
        try
        {
            if (!IsValidOperationObject(operation))
            {
                throw new Exception("Operation object not valid");
            }

            await _operationsRepository.AddOperation(true, operation);

            return true;
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<List<Operation>> GetCurrencyChangeLogs(string accountNumber)
    {
        try
        {
            return await _operationsRepository.GetCurrencyChangeLogs(accountNumber);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<List<Operation>> GetChargeAccountLogs(string accountNumber)
    {
        try
        {
            return await _operationsRepository.GetChargeAccountLogs(accountNumber);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<List<Operation>> GetTransactionsToCardLogs(string accountNumber)
    {
        try
        {
            return await _operationsRepository.GetTransactionsToCardLogs(accountNumber);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<List<Operation>> GetAllLogs(string accountNumber, int periodAsMonth)
    {
        try
        {
            return await _operationsRepository.GetAllLogs(accountNumber, periodAsMonth);
        }
        catch (Exception e)
        {
            throw new Exception();
        }
    }

    public async Task<Operation> BuildChargeOperation(BankAccount account, decimal amount)
    {
        try
        {
            var operationStr = new Operation()
            {
                OperationType = EnumOperationType.Deposit,
                Amount = amount,
                Description = $"Charged bank account with {Global.FormatCurrency(account.Currency, amount)}",
                OperationId = await GenerateUniqueRandomOperationIdAsync(),
                DateTime = DateTime.UtcNow,
                AccountId = account.NationalId,
                Currency = account.Currency,
                AccountNumber = account.AccountNumber
            };

            return operationStr;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Operation> BuildTransferOperation(BankAccount account, decimal amount, EnumOperationType type)
    {
        try
        {
            string summory = "";
            if (type is EnumOperationType.TransactionToAccount)
            {
                summory = "Transaction to account from your card";
            }

            if (type is EnumOperationType.TransactionToCard)
            {
                summory = "Transaction to card from your account";
            }

            if (type is EnumOperationType.TransactionCardToCard)
            {
                summory = "Transaction between internal cards";
            }

            var operationStr = new Operation()
            {
                OperationType = type,
                Amount = amount,
                Description = $"{summory} by {Global.FormatCurrency(account.Currency, amount)}",
                OperationId = await GenerateUniqueRandomOperationIdAsync(),
                DateTime = DateTime.UtcNow,
                AccountId = account.NationalId,
                Currency = account.Currency,
                AccountNumber = account.AccountNumber
            };

            return operationStr;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Operation> BuildStockOperation(BankAccount account, int number, string symbol, string corpName,
        decimal price, EnumOperationType operationType)
    {
        try
        {
            string desc = "";
            decimal totalPrice = number * price;
            string stocks = "stocks";
            if (number == 1)
            {
                stocks = "stock";
            }

            if (operationType is EnumOperationType.StockBuy)
            {
                desc
                    = $"Bought {number} {stocks} with total price {Global.FormatCurrency(account.Currency, totalPrice)} as {corpName} stock";
            }

            if (operationType is EnumOperationType.StockSell)
            {
                desc
                    = $"Sold {number} {stocks} with total price {Global.FormatCurrency(account.Currency, totalPrice)} as {corpName} stock";
            }

            var operationStr = new Operation()
            {
                OperationType = operationType,
                Amount = totalPrice,
                Description = desc,
                OperationId = await GenerateUniqueRandomOperationIdAsync(),
                DateTime = DateTime.UtcNow,
                AccountId = account.NationalId,
                Currency = account.Currency,
                AccountNumber = account.AccountNumber
            };

            return operationStr;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Operation> BuildDeleteOrCreateCardOperation(BankAccount account, Card card,
        EnumOperationType operationType)
    {
        try
        {
            string desc = "";
            if (operationType == EnumOperationType.CreateCard)
            {
                desc = $"Created {card.CardType} with ID number: {card.CardId}";
            }

            if (operationType == EnumOperationType.DeleteCard)
            {
                desc = $"Deleted {card.CardType} with ID number: {card.CardId}";
            }

            var operationStr = new Operation()
            {
                OperationType = operationType,
                Amount = 0,
                Description = desc,
                OperationId = await GenerateUniqueRandomOperationIdAsync(),
                DateTime = DateTime.UtcNow,
                AccountId = account.NationalId,
                Currency = account.Currency,
                AccountNumber = account.AccountNumber,
            };

            return operationStr;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Operation> BuildExchangeOperation(BankAccount account, Card? baseCard, Card? targetCard,
        decimal amount, EnumOperationType type)
    {
        try
        {
            string summory = "";
            EnumCurrency baseCurrnecy = EnumCurrency.SAR;
            EnumCurrency targetCurrnecy = EnumCurrency.SAR;
            string from = "Card";
            string to = "Card";
            if (type is EnumOperationType.ExchangeToAccount)
            {
                to = "Account";
                baseCurrnecy = baseCard.Currency;
                targetCurrnecy = account.Currency;
                summory = "Exchange to account from your card";
            }

            if (type is EnumOperationType.ExchangeToCard)
            {
                from = "Account";
                baseCurrnecy = account.Currency;
                targetCurrnecy = targetCard.Currency;
                summory = "Exchange to card from your account";
            }

            if (type is EnumOperationType.ExchangeCardToCard)
            {
                baseCurrnecy = baseCard.Currency;
                targetCurrnecy = targetCard.Currency;
                summory = "Exchange between internal cards";
            }

            var operationStr = new Operation()
            {
                OperationType = type,
                Amount = amount,
                Description
                    = $"{summory} by {Global.FormatCurrency(baseCurrnecy, amount)} from {from} to {to}, " +
                      $"{Global.FormatCurrency(baseCurrnecy)} to {Global.FormatCurrency(targetCurrnecy)}",
                OperationId = await GenerateUniqueRandomOperationIdAsync(),
                DateTime = DateTime.UtcNow,
                AccountId = account.NationalId,
                Currency = account.Currency,
                AccountNumber = account.AccountNumber
            };

            return operationStr;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
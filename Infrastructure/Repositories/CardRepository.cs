using Core.Entities;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CardRepository : ICardRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CardRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Card>> GetAllCards(string accountNumber)
    {
        try
        {
            List<Card> cards = await _dbContext.BankCards
                .Where(c => c.BankAccount.AccountNumber == accountNumber).ToListAsync();

            return cards;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<bool> IsUserHasCardWithInTypeAsync(string accountNumber, EnumCardType cardType)
    {
        var result = await _dbContext.BankCards.Where(c =>
            c.BankAccount.AccountNumber == accountNumber && c.CardType == cardType).AnyAsync();

        return result;
    }

    public async Task<bool> CreateCardAsync(Card card, BankAccount bankAccount)
    {
        try
        {
            var result = await _dbContext.BankCards.AddAsync(card);
            // if (result.State != EntityState.Added)
            // {
            //     return false;
            // }

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("Some error occurred: " + e.StackTrace);
        }
    }

    public async Task<Card> GetCardDetails(string accountNumber, int cardId)
    {
        try
        {
            var cardDetails = await _dbContext.BankCards
                .Where(c => c.CardId == cardId && c.BankAccount.AccountNumber == accountNumber)
                .FirstOrDefaultAsync();

            if (cardDetails == null)
            {
                throw new Exception($"Card with ID {cardId} not found for account {accountNumber}.");

            }

            return cardDetails;
        }
        catch (Exception e)
        {
            throw new Exception($"No cards under this ID number available for account {accountNumber}.");
        }
    }

    public async Task<bool> ChargeCardBalanceAsync(string accountNumber, int cardNumber, decimal amount)
    {
        try
        {
            var cardDetails = await GetCardDetails(accountNumber, cardNumber);
            cardDetails.Balance += amount;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("Error occurred: ", e);
        }
    }

    public async Task<bool> IsOpenedForOnlinePurchase(string accountNumber, int cardNumber)
    {
        try
        {
            var result = await _dbContext.BankCards.Where(c =>
                c.BankAccount.AccountNumber == accountNumber && c.CardId == cardNumber).FirstAsync();

            if (!result.OpenedForOnlinePurchase)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            throw new Exception("Error occurred: ", e);
        }
    }

    public async Task<bool> IsOpenedForInternalOperations(string accountNumber, int cardNumber)
    {
        try
        {
            var result = await _dbContext.BankCards.Where(c =>
                c.BankAccount.AccountNumber == accountNumber && c.CardId == cardNumber).FirstAsync();

            if (!result.OpenedForInternalOperations)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            throw new Exception("Error occurred: ", e);
        }
    }

    public async Task<bool> IsActivated(string accountNumber, int cardId)
    {
        try
        {
            var result = await _dbContext.BankCards.Where(c => c.CardId == cardId).FirstAsync();
            return result.IsActivated;
        }
        catch (Exception e)
        {
            throw new Exception("Error occurred: ", e);
        }
    }

    public async Task<bool> IsCardIdUnique(int cardId)
    {
        return !await _dbContext.BankCards.AnyAsync(c => c.CardId == cardId);
    }

    public async Task<bool> DeleteCard(int cardId)
    {
        try
        {
            var aimedCard = await _dbContext.BankCards.FirstAsync(c => c.CardId == cardId);
            _dbContext.BankCards.Remove(aimedCard);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<bool> ChangeCurrencyAsync(bool saveAsync,EnumCurrency currency, int cardId)
    {
        try
        {
            var aimedCard = await _dbContext.BankCards.FirstAsync(c => c.CardId == cardId);
            aimedCard.Currency = currency;
            if (saveAsync)
            {
                await _dbContext.SaveChangesAsync();
            }
            return true;
        }
        catch (Exception e)
        {
            throw new Exception("", e);
        }
    }

    public async Task<(bool isSuccess, decimal amountAfterExchange)> ChangeBalance(bool saveAsync,decimal newBalance,
        int cardId)
    {
        try
        {
            var cardDetails = await _dbContext.BankCards.FirstAsync(a => a.CardId == cardId);

            cardDetails.Balance = newBalance;
            if (saveAsync)
            {
                await _dbContext.SaveChangesAsync();
            }
            return (true, cardDetails.Balance);
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> HasThreeCards(string accountNumber)
    {
        try
        {
            return await _dbContext.BankCards
                .Where(c => c.BankAccount.AccountNumber == accountNumber).Take(3).CountAsync() == 3;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
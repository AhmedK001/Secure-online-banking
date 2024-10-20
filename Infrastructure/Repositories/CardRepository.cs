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
            List<Card> cards = await _dbContext.BankCards.Where(c => c.BankAccount.AccountNumber == accountNumber)
                .ToListAsync();

            return cards;
        }
        catch (Exception e)
        {
            return new List<Card>();
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
            throw new Exception("Some error occurred: " +e.StackTrace);
        }
    }

    public async Task<Card> GetCardDetails(string accountNumber, int cardId)
    {
        try
        {
            return await _dbContext.BankCards.Where(c =>c.CardId == cardId && c.BankAccount.AccountNumber == accountNumber).FirstAsync();
        }
        catch (Exception e)
        {
            return new Card();
        }
    }

    public async Task<bool> UpdateCardBalanceAsync(string accountNumber, int cardNumber,
        decimal amount)
    {
        try
        {
            var cardDetails = await GetCardDetails(accountNumber,cardNumber);
            cardDetails.Balance += amount;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            return false;
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
            return false;
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
            return false;
        }
    }

    public Task<bool> IsActivated(string accountNumber, int cardNumber)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsCardIdUnique(int cardId)
    {
        return !await _dbContext.BankCards.AnyAsync(c => c.CardId == cardId);
    }
}
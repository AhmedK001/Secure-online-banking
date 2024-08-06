using Core.Enums;
using Core.Interfaces;

namespace Core.Entities;

public class Card : ICard
{
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public int CardId { get; set; }
    public string CardNumber { get; set; }
    public DateTime expiryDate { get; set; }
    public EnumCardType CardType { get; set; }
    
    
    public void Activate()
    {
        throw new NotImplementedException();
    }

    public void DeActivate()
    {
        throw new NotImplementedException();
    }

    public string GetCardInfo()
    {
        throw new NotImplementedException();
    }
    
    public string GetCardInfo(string accountNumber,int cardId)
    {
        throw new NotImplementedException();
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;
using Core.Interfaces;

namespace Core.Entities;

[Table("Cards")]
public class BankCard : ICard
{
    public string AccountNumber { get; set; }
    public int CardId { get; set; }
    public string CardNumber { get; set; }
    public int CVV { get; set; }
    public DateTime ExpiryDate { get; set; }
    public EnumCardType CardType { get; set; }
    public bool OpenedForOnlinePurchase { get; set; }
    public bool OpenedForPhysicalOperations { get; set; }
    public decimal Balance { get; set; }
    public BankAccount BankAccount { get; set; }
    public List<Payment> Payments { get; set; }

    
    
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
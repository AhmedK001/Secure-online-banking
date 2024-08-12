using Core.Enums;

namespace Core.Interfaces;

public interface ICard
{
    string AccountNumber { get; set; }
    decimal Balance { get; set; }
    int CardId { get; set; }
    string CardNumber { get; set; }
    DateTime ExpiryDate { get; set; }
    EnumCardType CardType { get; set; }
    
    bool OpenedForOnlinePurchase { get; set; }
    bool OpenedForPhysicalOperations { get; set; }

    void Activate();
    void DeActivate();
    string GetCardInfo();
}
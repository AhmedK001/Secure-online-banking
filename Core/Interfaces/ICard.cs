using Core.Enums;

namespace Core.Interfaces;

public interface ICard
{
    string AccountNumber { get; set; }
    decimal Balance { get; set; }
    int CardId { get; set; }
    string CardNumber { get; set; }
    DateTime expiryDate { get; set; }
    EnumCardType CardType { get; set; }

    void Activate();
    void DeActivate();
    string GetCardInfo();
}
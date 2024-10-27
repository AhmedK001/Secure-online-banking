using Core.Entities;
using Core.Enums;

namespace Core.Interfaces;

public interface ICard
{
    public int CardId { get; set; }
    public int Cvv { get; set; }
    public DateTime ExpiryDate { get; set; }
    public EnumCardType CardType { get; set; }
    public bool OpenedForOnlinePurchase { get; set; }
    public bool OpenedForInternalOperations { get; set; }
    public bool IsActivated { get; set; }
    public decimal Balance { get; set; }
    public EnumCurrency Currency { get; set; }
    public BankAccount BankAccount { get; set; }
    public List<Payment>? Payments { get; set; }
}
using Core.Enums;

namespace Application.DTOs;

public class CardResponseDto
{
    public int CardId { get; set; }
    public int Cvv { get; set; }
    public DateTime ExpiryDate { get; set; }
    public EnumCardType CardType { get; set; }
    public bool OpenedForOnlinePurchase { get; set; }
    public bool OpenedForInternalOperations { get; set; }
    public bool IsActivated { get; set; }
    public decimal Balance { get; set; }
}
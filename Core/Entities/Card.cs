using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;
using Core.Interfaces;
using Newtonsoft.Json;

namespace Core.Entities;

[Table("Cards")]
public class Card : ICard
{
    public int CardId { get; set; }
    public int Cvv { get; set; }
    public DateTime ExpiryDate { get; set; }
    public EnumCardType CardType { get; set; }
    public bool OpenedForOnlinePurchase { get; set; }
    public bool OpenedForInternalOperations { get; set; }
    public bool IsActivated { get; set; }
    public decimal Balance { get; set; }

    // navigation props
    [JsonIgnore]
    public BankAccount BankAccount { get; set; }
    [JsonIgnore]
    public List<Payment>? Payments { get; set; }
}
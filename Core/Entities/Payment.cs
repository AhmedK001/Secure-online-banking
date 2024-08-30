using Core.Interfaces;

namespace Core.Entities;

public class Payment : IPayment
{
    public int PaymentId { get; set; }
    public int CardId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public BankCard Card { get; set; }
}
namespace Core.Interfaces;

public interface IPayment
{
    int CardId { get; set; }
    int PaymentId { get; set; }
    decimal Amount { get; set; }
    DateTime DateTime { get; set; }
}
using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Interfaces;

namespace Core.Entities;

public class Operation : IOperation
{
    public string AccountNumber { get; set; }
    public int AccountId { get; set; }
    public int OperationId { get; set; }
    public EnumCurrency Currency { get; set; }
    public DateTime DateTime { get; set; }
    public EnumOperationType OperationType { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public ReceiverClient Receiver { get; set; }
}
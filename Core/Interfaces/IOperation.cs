using Core.Entities;
using Core.Enums;

namespace Core.Interfaces;

public interface IOperation
{
    string AccountNumber { get; set; }
    int AccountId { get; set; }
    int OperationId { get; set; }
    EnumOperationType OperationType { get; set; }
    decimal Amount { get; set; }
    EnumCurrency Currency { get; set; }
    DateTime DateTime { get; set; }
    ReceiverClient? Receiver { get; set; }
}
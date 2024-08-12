using Core.Entities;
using Core.Enums;

namespace Core.Interfaces;

public interface IOperation
{
    int AccountId { get; set; }
    string AccountNumber { get; set; }
    int OperationId { get; set; }
    DateTime DateTime { get; set; }
    EnumOperationType OperationType { get; set; }
    decimal Amount { get; set; }
    ReceiverClient Receiver { get; set; }
    
    IOperation GetTransactionInfo();
}
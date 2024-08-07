using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Interfaces;

namespace Core.Entities;

public class Operation : IOperation
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; }
    public int OperationId { get; set; }
    public DateTime DateTime { get; set; }
    public EnumOperationType OperationType { get; set; }
    public decimal Amount { get; set; }
    
    public IOperation GetTransactionInfo()
    {
        throw new NotImplementedException();
    }
}
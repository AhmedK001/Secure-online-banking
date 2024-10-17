namespace Core.Interfaces;

public interface IReceiverClient
{
    Guid? ReceiverId { get; set; }
    string ReceiverAccountNumber { get; set; }
    string FullName { get; set; }
    int OperationId { get; set; }
    decimal ReceivedAmount { get; set; }
    IOperation GetOperationReport();
    IReceiverClient GetReceiverClientInfo();
}
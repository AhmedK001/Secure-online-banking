namespace Core.Interfaces;

public interface IReceiverClient
{
    string AccountNumber { get; set; }
    string FullName { get; set; }

    void DeductBalance();
    void AddBalance();
    ITransaction GetOperationReport();
    IReceiverClient GetReceiverClientInfo();
}
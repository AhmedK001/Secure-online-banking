﻿namespace Core.Interfaces;

public interface IReceiverClient
{
    string AccountNumber { get; set; }
    string FullName { get; set; }
    int OperationId { get; set; }
    void DeductBalance();
    void AddBalance();
    IOperation GetOperationReport();
    IReceiverClient GetReceiverClientInfo();
}
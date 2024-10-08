﻿using System.ComponentModel.DataAnnotations;
using Core.Interfaces;

namespace Core.Entities;

public class ReceiverClient : IReceiverClient
{
    public int OperationId { get; set; }
    public string AccountNumber { get; set; }
    public string FullName { get; set; }
    public Operation Operation { get; set; }
    
    
    public void DeductBalance()
    {
        throw new NotImplementedException();
    }

    public void AddBalance()
    {
        throw new NotImplementedException();
    }

    public IOperation GetOperationReport()
    {
        throw new NotImplementedException();
    }

    public IReceiverClient GetReceiverClientInfo()
    {
        throw new NotImplementedException();
    }
}
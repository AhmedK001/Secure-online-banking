using Core.Entities;

namespace Application.Interfaces;

public interface IExcelService
{
    Task<StreamContent> GetAllOperations(List<Operation> operations, User user, BankAccount bankAccount);
}
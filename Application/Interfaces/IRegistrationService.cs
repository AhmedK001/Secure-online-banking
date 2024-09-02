
using Core.Entities;
namespace Application.Interfaces;

public interface IRegistrationService
{
    Task<string> RegisterUser(User user);
    
}
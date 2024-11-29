using Core.Entities;

namespace Application.Interfaces;

public interface ISearchUserService
{
    Task<User> FindUser(int nationalId);
    bool CheckIfNationalIdUnique(string nationalId);
    bool CheckIfEmailUnique(string email);
    bool CheckIfPhoneUnique(string phone);
}
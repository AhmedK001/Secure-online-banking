using Application.Interfaces;
using Core.Entities;
using Core.Interfaces.IRepositories;

namespace Application.Services;

public class SearchUserService : ISearchUserService
{
    private readonly IUserRepository _userRepository;

    public SearchUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task<User> FindUser(int nationalId)
    {
        // var result = await _userRepository.FindUserAsyncByNationalId((Guid)nationalId);
        // return result;
        return null;
    }

    public bool CheckIfNationalIdUnique(string nationalId)
    {
        return _userRepository.CheckIfNationalIdUnique(nationalId);
    }

    public bool CheckIfEmailUnique(string email)
    {
        return _userRepository.CheckIfEmailUnique(email);
    }

    public bool CheckIfPhoneUnique(string phone)
    {
        return _userRepository.CheckIfPhoneUnique(phone);
    }
}
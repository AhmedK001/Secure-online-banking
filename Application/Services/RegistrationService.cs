using Application.Interfaces;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.IRepositories;

namespace Application.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IUserRepository _userRepository;

    public RegistrationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<string> RegisterUser(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<string> RegisterUserAsync(User user)
    {
        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return $"Your data has been saved successfully for user: {user.FirstName} {user.LastName} " +
               $"With National Id number {user.NationalId}";
    }

}


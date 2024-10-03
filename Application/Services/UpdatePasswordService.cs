using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;


public class UpdatePasswordService : IUpdatePassword
{
    private readonly UserManager<User> _userManager;


    public UpdatePasswordService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }


    public async Task<IdentityResult> UpdatePasswordAsync(User user, UpdatePasswordDto updatePasswordDto)
    {

        // check if current password matches
        var checkCurrentPassword
            = await _userManager.CheckPasswordAsync(user,
                updatePasswordDto.CurrentPassword);
        if (!checkCurrentPassword) return IdentityResult.Failed(new IdentityError
        {
            Description = "Current password is incorrect."
        });

        // update password
        var changePasswordResult = await _userManager.ChangePasswordAsync(user,
            updatePasswordDto.CurrentPassword, updatePasswordDto.Password);

        return changePasswordResult;
    }
}
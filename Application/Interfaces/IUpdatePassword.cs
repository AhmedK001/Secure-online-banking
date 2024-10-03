using Application.DTOs;
using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces;

public interface IUpdatePassword
{
    Task<IdentityResult> UpdatePasswordAsync(User user, UpdatePasswordDto updatePasswordDto);
}
using System.Security.Claims;
using Core.Entities;

namespace Application.Interfaces;

public interface IAccountSettingsService
{
    Task<(bool isSuccess,string errorMessage)> EnableNotificationsAsync(User user);
    Task<(bool isSuccess,string errorMessage)> DisableNotificationsAsync(User user);
}
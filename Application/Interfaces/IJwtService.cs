using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface IJwtService
{
    AuthenticationResponse CreateJwtToken(User user);
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public AuthenticationResponse CreateJwtToken(User user)
    {
        if (user == null)
        {
            throw new Exception();
        }
        Console.WriteLine(user);
        
        DateTime expirationTime = DateTime.UtcNow.AddMinutes(
            Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));
        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTime.UtcNow.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserName.ToString())
        };

        SymmetricSecurityKey securityKey
            = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:KEY"]));

        SigningCredentials signingCredentials
            = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);

        JwtSecurityToken tokenGenerator = new JwtSecurityToken(
            _configuration["Jwt:ISSUER"], _configuration["Jwt:AUDIENCE"],
            claims, expires: expirationTime,
            signingCredentials: signingCredentials);

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string token = tokenHandler.WriteToken(tokenGenerator);

        return new AuthenticationResponse()
        {
            Token = token,
            EmailAddress = user.Email,
            UserName = user.UserName,
            ExpirationTime = expirationTime
        };

    }
}
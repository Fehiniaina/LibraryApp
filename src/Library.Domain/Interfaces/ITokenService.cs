// src/Library.Application/Common/Interfaces/ITokenService.cs
namespace Library.Domain.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string email, IList<string> roles);
    string GenerateRefreshToken();
}
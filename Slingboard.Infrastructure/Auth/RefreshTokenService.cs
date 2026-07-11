using Slingboard.Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Slingboard.Infrastructure.Auth;

public class RefreshTokenService : IRefreshTokenService
{
    public (string rawToken, string tokenHash) GenerateToken()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (rawToken, Hash(rawToken));
    }

    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}
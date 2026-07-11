namespace Slingboard.Application.Common.Interfaces;

public interface IRefreshTokenService
{
    (string rawToken, string tokenHash) GenerateToken();
    string Hash(string rawToken);
}
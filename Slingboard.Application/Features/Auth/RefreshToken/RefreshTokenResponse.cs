namespace Slingboard.Application.Features.Auth.RefreshToken;

public record RefreshTokenResponse(string AccessToken, int ExpiresIn, string RefreshToken);
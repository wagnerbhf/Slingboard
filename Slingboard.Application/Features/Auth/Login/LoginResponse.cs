namespace Slingboard.Application.Features.Auth.Login;

public record LoginResponse(string AccessToken, int ExpiresIn, string RefreshToken);
using Mediator;

namespace Slingboard.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RawRefreshToken) : IRequest<RefreshTokenResponse>;
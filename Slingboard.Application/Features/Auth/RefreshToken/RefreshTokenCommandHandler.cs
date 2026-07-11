using Mediator;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(
    IAppDbContext context,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async ValueTask<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RawRefreshToken))
            throw new UnauthorizedException("Refresh token ausente.");

        var incomingHash = refreshTokenService.Hash(request.RawRefreshToken);

        var existingToken = context.RefreshTokens.FirstOrDefault(rt => rt.TokenHash == incomingHash);

        if (existingToken is null || !existingToken.IsActive)
            throw new UnauthorizedException("Refresh token inválido ou expirado.");

        var user = context.Users.FirstOrDefault(u => u.Id == existingToken.UserId);
        if (user is null || !user.IsActive)
            throw new UnauthorizedException("Usuário inválido.");

        var (newRawToken, newTokenHash) = refreshTokenService.GenerateToken();
        existingToken.Revoke(newTokenHash);

        var newRefreshToken = Domain.Entities.RefreshToken.Create(user.Id, newTokenHash, DateTime.UtcNow.AddDays(7));
        context.RefreshTokens.Add(newRefreshToken);

        var newAccessToken = jwtTokenGenerator.GenerateAccessToken(user);

        await context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(newAccessToken, ExpiresIn: 900, newRawToken);
    }
}
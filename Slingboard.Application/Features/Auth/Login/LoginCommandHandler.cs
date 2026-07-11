using Mediator;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using RefreshTokenEntity = Slingboard.Domain.Entities.RefreshToken;

namespace Slingboard.Application.Features.Auth.Login;

public class LoginCommandHandler(
    IAppDbContext context,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async ValueTask<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Slingboard.Domain.ValueObjects.Email.Create(request.Email);
        var user = context.Users.FirstOrDefault(u => u.Email == email);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Email ou senha inválidos.");

        if (!user.IsActive)
            throw new UnauthorizedException("Usuário inativo.");

        user.RegisterLogin();

        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);

        var (rawToken, tokenHash) = refreshTokenService.GenerateToken();
        var refreshToken = RefreshTokenEntity.Create(user.Id, tokenHash, DateTime.UtcNow.AddDays(7));
        context.RefreshTokens.Add(refreshToken);

        await context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, ExpiresIn: 900, rawToken);
    }
}
using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Auth.Login;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();

    private LoginCommandHandler CreateHandler(TestDbContext context) =>
        new(context, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object, _refreshTokenServiceMock.Object);

    [Fact]
    public async Task Handle_ComCredenciaisValidas_DeveRetornarTokens()
    {
        await using var context = TestDbContextFactory.Create();
        var user = User.Create("João Silva", Email.Create("joao@teste.com"), "hashed-password");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _passwordHasherMock.Setup(h => h.Verify("SenhaForte123!", "hashed-password")).Returns(true);
        _jwtTokenGeneratorMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("fake-jwt-token");
        _refreshTokenServiceMock.Setup(r => r.GenerateToken()).Returns(("raw-token", "hashed-token"));

        var handler = CreateHandler(context);
        var command = new LoginCommand("joao@teste.com", "SenhaForte123!");

        var result = await handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("fake-jwt-token");
        result.RefreshToken.Should().Be("raw-token");
        result.ExpiresIn.Should().Be(900);
    }

    [Fact]
    public async Task Handle_ComSenhaErrada_DeveLancarUnauthorizedException()
    {
        await using var context = TestDbContextFactory.Create();
        var user = User.Create("João Silva", Email.Create("joao@teste.com"), "hashed-password");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var handler = CreateHandler(context);
        var command = new LoginCommand("joao@teste.com", "SenhaErrada!");

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ComEmailInexistente_DeveLancarUnauthorizedException()
    {
        await using var context = TestDbContextFactory.Create();

        var handler = CreateHandler(context);
        var command = new LoginCommand("naoexiste@teste.com", "SenhaForte123!");

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ComUsuarioInativo_DeveLancarUnauthorizedException()
    {
        await using var context = TestDbContextFactory.Create();
        var user = User.Create("João Silva", Email.Create("joao@teste.com"), "hashed-password");
        user.Deactivate();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var handler = CreateHandler(context);
        var command = new LoginCommand("joao@teste.com", "SenhaForte123!");

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*inativo*");
    }
}
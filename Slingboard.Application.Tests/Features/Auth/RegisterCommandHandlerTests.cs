using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Auth.Register;
using Slingboard.Application.Tests.Common;

namespace Slingboard.Application.Tests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();

    [Fact]
    public async Task Handle_ComEmailNovo_DeveCriarUsuarioComSucesso()
    {
        await using var context = TestDbContextFactory.Create();
        _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");

        var handler = new RegisterCommandHandler(context, _passwordHasherMock.Object);
        var command = new RegisterCommand("João Silva", "joao@teste.com", "SenhaForte123!");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("João Silva");
        result.Email.Should().Be("joao@teste.com");
        context.Users.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ComEmailJaExistente_DeveLancarConflictException()
    {
        await using var context = TestDbContextFactory.Create();
        _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");

        var handler = new RegisterCommandHandler(context, _passwordHasherMock.Object);
        var command = new RegisterCommand("João Silva", "joao@teste.com", "SenhaForte123!");
        await handler.Handle(command, CancellationToken.None);

        var duplicateCommand = new RegisterCommand("Outro Nome", "joao@teste.com", "OutraSenha123!");
        var act = () => handler.Handle(duplicateCommand, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<ConflictException>();
    }
}
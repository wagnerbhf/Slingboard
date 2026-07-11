using FluentAssertions;
using Slingboard.Domain.Entities;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriarUsuarioAtivo()
    {
        var email = Email.Create("joao@teste.com");

        var user = User.Create("João Silva", email, "hash123");

        user.Name.Should().Be("João Silva");
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be("hash123");
        user.IsActive.Should().BeTrue();
        user.LastLoginAt.Should().BeNull();
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ComNomeVazio_DeveLancarExcecao()
    {
        var email = Email.Create("joao@teste.com");

        var act = () => User.Create("", email, "hash123");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegisterLogin_DevePreencherLastLoginAt()
    {
        var user = User.Create("João Silva", Email.Create("joao@teste.com"), "hash123");

        user.RegisterLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Deactivate_DeveMarcarComoInativo()
    {
        var user = User.Create("João Silva", Email.Create("joao@teste.com"), "hash123");

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }
}
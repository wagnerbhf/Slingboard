using FluentAssertions;
using Slingboard.Domain.Exceptions;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("joao@teste.com")]
    [InlineData("Maria.Souza@Empresa.COM.BR")]
    public void Create_ComEmailValido_DeveCriarComSucesso(string input)
    {
        var email = Email.Create(input);

        email.Value.Should().Be(input.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("emailinvalido")]
    [InlineData("email@")]
    [InlineData("@dominio.com")]
    public void Create_ComEmailInvalido_DeveLancarExcecao(string input)
    {
        var act = () => Email.Create(input);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Create_DeveNormalizarParaLowerCase()
    {
        var email = Email.Create("JOAO@TESTE.COM");

        email.Value.Should().Be("joao@teste.com");
    }

    [Fact]
    public void Equals_ComMesmoValor_DeveSerIgual()
    {
        var email1 = Email.Create("joao@teste.com");
        var email2 = Email.Create("JOAO@teste.com");

        email1.Should().Be(email2);
        (email1 == null!).Should().BeFalse();
    }
}
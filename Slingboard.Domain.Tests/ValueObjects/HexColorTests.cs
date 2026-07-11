using FluentAssertions;
using Slingboard.Domain.Exceptions;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Domain.Tests.ValueObjects;

public class HexColorTests
{
    [Theory]
    [InlineData("#FF5733")]
    [InlineData("#000000")]
    [InlineData("#ffffff")]
    public void Create_ComCorValida_DeveCriarComSucesso(string input)
    {
        var color = HexColor.Create(input);

        color.Value.Should().Be(input.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("azul")]
    [InlineData("#FFF")]
    [InlineData("FF5733")]
    [InlineData("#GGGGGG")]
    public void Create_ComCorInvalida_DeveLancarExcecao(string input)
    {
        var act = () => HexColor.Create(input);

        act.Should().Throw<BusinessRuleViolationException>();
    }
}
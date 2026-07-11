using FluentAssertions;
using Slingboard.Domain.Entities;
using Slingboard.Domain.Exceptions;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Domain.Tests.Entities;

public class LabelTests
{
    [Fact]
    public void Update_DeveAtualizarNomeECor()
    {
        var board = Board.Create("Board", Guid.NewGuid());
        var label = board.AddLabel("Backend", HexColor.Create("#22C55E"));

        label.Update("Backend API", HexColor.Create("#16A34A"));

        label.Name.Should().Be("Backend API");
        label.Color.Value.Should().Be("#16A34A");
    }

    [Fact]
    public void Update_ComNomeMaiorQue30Caracteres_DeveLancarExcecao()
    {
        var board = Board.Create("Board", Guid.NewGuid());
        var label = board.AddLabel("Backend", HexColor.Create("#22C55E"));
        var nomeGrande = new string('A', 31);

        var act = () => label.Update(nomeGrande, HexColor.Create("#22C55E"));

        act.Should().Throw<BusinessRuleViolationException>();
    }
}
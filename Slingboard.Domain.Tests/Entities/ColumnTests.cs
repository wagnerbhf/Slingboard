using FluentAssertions;
using Slingboard.Domain.Entities;

namespace Slingboard.Domain.Tests.Entities;

public class ColumnTests
{
    [Fact]
    public void UpdateDetails_DeveAtualizarTituloELimite()
    {
        var board = Board.Create("Board", Guid.NewGuid());
        var column = board.Columns.First();

        column.UpdateDetails("Novo Título", 10);

        column.Title.Should().Be("Novo Título");
        column.Limit.Should().Be(10);
    }

    [Fact]
    public void UpdateOrder_DeveAtualizarPosicao()
    {
        var board = Board.Create("Board", Guid.NewGuid());
        var column = board.Columns.First();

        column.UpdateOrder(5);

        column.Order.Should().Be(5);
    }
}
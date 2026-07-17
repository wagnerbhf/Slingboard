using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Columns.Commands.UpdateColumn;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;

namespace Slingboard.Application.Tests.Features.Columns;

public class UpdateColumnCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAtualizarTituloELimite()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var coluna = board.Columns.First();
        var handler = new UpdateColumnCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new UpdateColumnCommand(coluna.Id, "To Do - Backlog", 8);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("To Do - Backlog");
        result.Limit.Should().Be(8);
    }

    [Fact]
    public async Task Handle_ComColunaInexistente_DeveLancarNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        _currentUserMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var handler = new UpdateColumnCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new UpdateColumnCommand(Guid.NewGuid(), "Título", null);

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ComUsuarioNaoMembro_DeveLancarForbiddenException()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var outsiderId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();
        var coluna = board.Columns.First();

        _currentUserMock.Setup(c => c.UserId).Returns(outsiderId);
        var handler = new UpdateColumnCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new UpdateColumnCommand(coluna.Id, "Novo Título", null);

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
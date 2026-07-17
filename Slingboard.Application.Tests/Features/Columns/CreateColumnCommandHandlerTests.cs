using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Columns.Commands.CreateColumn;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;

namespace Slingboard.Application.Tests.Features.Columns;

public class CreateColumnCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCriarColunaComOrderIncrementado()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var handler = new CreateColumnCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new CreateColumnCommand(board.Id, "Em Revisão", 5);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("Em Revisão");
        result.Order.Should().Be(3);
        result.Limit.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ComBoardInexistente_DeveLancarNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        _currentUserMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var handler = new CreateColumnCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new CreateColumnCommand(Guid.NewGuid(), "Em Revisão", null);

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

        _currentUserMock.Setup(c => c.UserId).Returns(outsiderId);
        var handler = new CreateColumnCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new CreateColumnCommand(board.Id, "Em Revisão", null);

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
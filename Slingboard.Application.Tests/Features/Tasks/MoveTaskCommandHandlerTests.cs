using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Tasks.Commands.MoveTask;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;

namespace Slingboard.Application.Tests.Features.Tasks;

public class MoveTaskCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_MoverParaOutraColuna_DeveAtualizarColumnIdEOrder()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);

        var colunaOrigem = board.Columns.ElementAt(0);
        var colunaDestino = board.Columns.ElementAt(1);

        var task = TaskItem.Create(board.Id, colunaOrigem.Id, "Task 1", ownerId, order: 0);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new MoveTaskCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new MoveTaskCommand(task.Id, colunaDestino.Id, 0);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ColumnId.Should().Be(colunaDestino.Id);
        result.Order.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReordenarNaMesmaColuna_DeveAjustarOrdersCorretamente()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        var coluna = board.Columns.ElementAt(0);

        var task1 = TaskItem.Create(board.Id, coluna.Id, "Task 1", ownerId, order: 0);
        var task2 = TaskItem.Create(board.Id, coluna.Id, "Task 2", ownerId, order: 1);
        var task3 = TaskItem.Create(board.Id, coluna.Id, "Task 3", ownerId, order: 2);
        context.Tasks.AddRange(task1, task2, task3);
        await context.SaveChangesAsync();

        var handler = new MoveTaskCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new MoveTaskCommand(task3.Id, coluna.Id, 0);

        await handler.Handle(command, CancellationToken.None);

        var tasksReordenadas = context.Tasks.Where(t => t.ColumnId == coluna.Id).OrderBy(t => t.Order).ToList();
        tasksReordenadas.Select(t => t.Id).Should().ContainInOrder(task3.Id, task1.Id, task2.Id);
    }

    [Fact]
    public async Task Handle_ComColunaDestinoInexistente_DeveLancarNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        var coluna = board.Columns.ElementAt(0);
        var task = TaskItem.Create(board.Id, coluna.Id, "Task 1", ownerId, order: 0);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new MoveTaskCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new MoveTaskCommand(task.Id, Guid.NewGuid(), 0);

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
        var coluna = board.Columns.ElementAt(0);
        var task = TaskItem.Create(board.Id, coluna.Id, "Task 1", ownerId, order: 0);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(outsiderId);
        var handler = new MoveTaskCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new MoveTaskCommand(task.Id, board.Columns.ElementAt(1).Id, 0);

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_DeveChamarRealtimeNotifier()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        var coluna = board.Columns.ElementAt(0);
        var task = TaskItem.Create(board.Id, coluna.Id, "Task 1", ownerId, order: 0);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new MoveTaskCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new MoveTaskCommand(task.Id, board.Columns.ElementAt(1).Id, 0);

        await handler.Handle(command, CancellationToken.None);

        _realtimeNotifierMock.Verify(r => r.NotifyTaskMoved(
            board.Id, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
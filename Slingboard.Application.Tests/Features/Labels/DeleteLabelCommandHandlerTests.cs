using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Labels.Commands.DeleteLabel;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Tests.Features.Labels;

public class DeleteLabelCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_ComLabelAssociadaATask_DeveRemoverAssociacaoEDaLabel()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        var label = board.AddLabel("Backend", HexColor.Create("#22C55E"));
        context.Boards.Add(board);
        context.Labels.Add(label);

        var coluna = board.Columns.First();
        var task = Domain.Entities.TaskItem.Create(board.Id, coluna.Id, "Task 1", ownerId, order: 0);
        task.AddLabel(label.Id);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new DeleteLabelCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new DeleteLabelCommand(label.Id);

        await handler.Handle(command, CancellationToken.None);

        context.Labels.Should().BeEmpty();
        context.TaskLabels.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ComUsuarioNaoMembro_DeveLancarForbiddenException()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var outsiderId = Guid.NewGuid();

        var board = Board.Create("Board", ownerId);
        var label = board.AddLabel("Backend", HexColor.Create("#22C55E"));
        context.Boards.Add(board);
        context.Labels.Add(label);
        await context.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(outsiderId);
        var handler = new DeleteLabelCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new DeleteLabelCommand(label.Id);

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
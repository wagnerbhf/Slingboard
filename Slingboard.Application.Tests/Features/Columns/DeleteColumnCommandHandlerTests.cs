using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Columns.Commands.DeleteColumn;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;

namespace Slingboard.Application.Tests.Features.Columns;

public class DeleteColumnCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    [Fact]
    public async Task Handle_ComColunaVazia_DeveRemoverComSucesso()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var colunaParaRemover = board.Columns.ElementAt(2); // "Done"
        var handler = new DeleteColumnCommandHandler(context, _currentUserMock.Object);
        var command = new DeleteColumnCommand(colunaParaRemover.Id, null);

        await handler.Handle(command, CancellationToken.None);

        context.Boards.First(b => b.Id == board.Id).Columns.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ComTasksNaColuna_DeveRealocarParaOutraColuna()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);

        var colunaOrigem = board.Columns.ElementAt(0);
        var colunaDestino = board.Columns.ElementAt(1);
        var task = Slingboard.Domain.Entities.TaskItem.Create(board.Id, colunaOrigem.Id, "Task 1", ownerId, order: 0);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new DeleteColumnCommandHandler(context, _currentUserMock.Object);
        var command = new DeleteColumnCommand(colunaOrigem.Id, colunaDestino.Id);

        await handler.Handle(command, CancellationToken.None);

        var taskAtualizada = context.Tasks.First(t => t.Id == task.Id);
        taskAtualizada.ColumnId.Should().Be(colunaDestino.Id);
    }

    [Fact]
    public async Task Handle_QuandoRestaApenasUmaColuna_DeveLancarBusinessRuleViolationException()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        board.RemoveColumn(board.Columns.ElementAt(2).Id);
        board.RemoveColumn(board.Columns.ElementAt(1).Id);
        await context.SaveChangesAsync();

        var ultimaColuna = board.Columns.Single();
        var handler = new DeleteColumnCommandHandler(context, _currentUserMock.Object);
        var command = new DeleteColumnCommand(ultimaColuna.Id, null);

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<Domain.Exceptions.BusinessRuleViolationException>();
    }
}
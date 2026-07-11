using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Labels.Commands.CreateLabel;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;
using Slingboard.Domain.Exceptions;

namespace Slingboard.Application.Tests.Features.Labels;

public class CreateLabelCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCriarLabelComSucesso()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var handler = new CreateLabelCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new CreateLabelCommand(board.Id, "Backend", "#22C55E");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Backend");
        result.Color.Should().Be("#22C55E");
    }

    [Fact]
    public async Task Handle_ComNomeDuplicado_DeveLancarBusinessRuleViolationException()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var handler = new CreateLabelCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        await handler.Handle(new CreateLabelCommand(board.Id, "Backend", "#22C55E"), CancellationToken.None);

        var act = () => handler.Handle(new CreateLabelCommand(board.Id, "backend", "#000000"), CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task Handle_DeveChamarRealtimeNotifier()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        var handler = new CreateLabelCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        await handler.Handle(new CreateLabelCommand(board.Id, "Backend", "#22C55E"), CancellationToken.None);

        _realtimeNotifierMock.Verify(r => r.NotifyLabelCreated(
            board.Id, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Labels.Commands.UpdateLabel;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Tests.Features.Labels;

public class UpdateLabelCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IRealtimeNotifier> _realtimeNotifierMock = new();

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAtualizarNomeECor()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        var label = board.AddLabel("Backend", HexColor.Create("#22C55E"));
        context.Boards.Add(board);
        context.Labels.Add(label);
        await context.SaveChangesAsync();

        var handler = new UpdateLabelCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new UpdateLabelCommand(label.Id, "Backend API", "#16A34A");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Backend API");
        result.Color.Should().Be("#16A34A");
    }

    [Fact]
    public async Task Handle_ComLabelInexistente_DeveLancarNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        _currentUserMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var handler = new UpdateLabelCommandHandler(context, _currentUserMock.Object, _realtimeNotifierMock.Object);
        var command = new UpdateLabelCommand(Guid.NewGuid(), "Nome", "#000000");

        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
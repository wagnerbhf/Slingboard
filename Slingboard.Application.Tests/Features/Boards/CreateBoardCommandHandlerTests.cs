using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Boards.Commands.CreateBoard;
using Slingboard.Application.Tests.Common;

namespace Slingboard.Application.Tests.Features.Boards;

public class CreateBoardCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    [Fact]
    public async Task Handle_ComTituloValido_DeveCriarBoardComTresColunasPadrao()
    {
        await using var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(userId);

        var handler = new CreateBoardCommandHandler(context, _currentUserMock.Object);
        var command = new CreateBoardCommand("Meu Board", "Descrição", "#1E3A8A");
        var result = await handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("Meu Board");
        result.OwnerId.Should().Be(userId);
        result.Columns.Should().HaveCount(3);
        result.Columns.Select(c => c.Title).Should().ContainInOrder("To Do", "In Progress", "Done");
    }

    [Fact]
    public async Task Handle_SemBackgroundColor_DevePermitirNull()
    {
        await using var context = TestDbContextFactory.Create();
        _currentUserMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

        var handler = new CreateBoardCommandHandler(context, _currentUserMock.Object);
        var command = new CreateBoardCommand("Board Sem Cor", null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.BackgroundColor.Should().BeNull();
    }
}
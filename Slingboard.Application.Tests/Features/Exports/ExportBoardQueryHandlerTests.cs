using FluentAssertions;
using Moq;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Exports;
using Slingboard.Application.Features.Exports.Queries.ExportBoard;
using Slingboard.Application.Tests.Common;
using Slingboard.Domain.Entities;

namespace Slingboard.Application.Tests.Features.Exports;

public class ExportBoardQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ICsvExportService> _csvExportServiceMock = new();
    private readonly Mock<IPdfExportService> _pdfExportServiceMock = new();

    private ExportBoardQueryHandler CreateHandler(TestDbContext context) =>
        new(context, _currentUserMock.Object, _csvExportServiceMock.Object, _pdfExportServiceMock.Object);

    [Fact]
    public async Task Handle_ComFormatoCsv_DeveChamarCsvExportService()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        _csvExportServiceMock.Setup(c => c.Generate(It.IsAny<BoardExportData>())).Returns([1, 2, 3]);

        var handler = CreateHandler(context);
        var query = new ExportBoardQuery(board.Id, "csv", true, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ContentType.Should().Be("text/csv");
        result.FileName.Should().EndWith(".csv");
        _csvExportServiceMock.Verify(c => c.Generate(It.IsAny<BoardExportData>()), Times.Once);
        _pdfExportServiceMock.Verify(p => p.Generate(It.IsAny<BoardExportData>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComFormatoPdf_DeveChamarPdfExportService()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        _pdfExportServiceMock.Setup(p => p.Generate(It.IsAny<BoardExportData>())).Returns([1, 2, 3]);

        var handler = CreateHandler(context);
        var query = new ExportBoardQuery(board.Id, "pdf", true, null, null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ContentType.Should().Be("application/pdf");
        result.FileName.Should().EndWith(".pdf");
        _pdfExportServiceMock.Verify(p => p.Generate(It.IsAny<BoardExportData>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComIncludeCompletedFalse_DeveExcluirTasksDaColunaDone()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(ownerId);

        var board = Board.Create("Board", ownerId);
        context.Boards.Add(board);

        var colunaDone = board.Columns.First(c => c.Title == "Done");
        var colunaTodo = board.Columns.First(c => c.Title == "To Do");

        var taskConcluida = Domain.Entities.TaskItem.Create(board.Id, colunaDone.Id, "Task Concluída", ownerId, order: 0);
        var taskPendente = Domain.Entities.TaskItem.Create(board.Id, colunaTodo.Id, "Task Pendente", ownerId, order: 0);
        context.Tasks.AddRange(taskConcluida, taskPendente);
        await context.SaveChangesAsync();

        BoardExportData? capturedData = null;
        _csvExportServiceMock
            .Setup(c => c.Generate(It.IsAny<BoardExportData>()))
            .Callback<BoardExportData>(data => capturedData = data)
            .Returns([1]);

        var handler = CreateHandler(context);
        var query = new ExportBoardQuery(board.Id, "csv", false, null, null);

        await handler.Handle(query, CancellationToken.None);

        capturedData.Should().NotBeNull();
        capturedData!.TotalTasks.Should().Be(1);
        capturedData.Columns.SelectMany(c => c.Tasks).Should().ContainSingle(t => t.Title == "Task Pendente");
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
        var handler = CreateHandler(context);
        var query = new ExportBoardQuery(board.Id, "csv", true, null, null);

        var act = () => handler.Handle(query, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
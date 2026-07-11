using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Exports.Queries.ExportBoard;

public class ExportBoardQueryHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    ICsvExportService csvExportService,
    IPdfExportService pdfExportService) : IRequestHandler<ExportBoardQuery, ExportFileResult>
{
    public async ValueTask<ExportFileResult> Handle(ExportBoardQuery request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Members)
            .Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Apenas membros do board podem exportar.");

        var tasksQuery = context.Tasks
            .Include(t => t.Labels)
            .Where(t => t.BoardId == request.BoardId);

        if (request.DateFrom.HasValue)
            tasksQuery = tasksQuery.Where(t => t.CreatedAt >= request.DateFrom);

        if (request.DateTo.HasValue)
            tasksQuery = tasksQuery.Where(t => t.CreatedAt <= request.DateTo);

        var tasks = await tasksQuery.ToListAsync(cancellationToken);

        var doneColumnIds = board.Columns
            .Where(c => c.Title.Equals("Done", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Id)
            .ToHashSet();

        if (!request.IncludeCompleted)
            tasks = tasks.Where(t => !doneColumnIds.Contains(t.ColumnId)).ToList();

        var assigneeIds = tasks.Where(t => t.AssigneeId.HasValue).Select(t => t.AssigneeId!.Value).Distinct().ToList();
        var assignees = await context.Users.Where(u => assigneeIds.Contains(u.Id)).ToListAsync(cancellationToken);

        var allLabelIds = tasks.SelectMany(t => t.Labels.Select(l => l.LabelId)).Distinct().ToList();
        var labels = await context.Labels.Where(l => allLabelIds.Contains(l.Id)).ToListAsync(cancellationToken);

        var columnsData = board.Columns.OrderBy(c => c.Order).Select(column =>
        {
            var columnTasks = tasks
                .Where(t => t.ColumnId == column.Id)
                .OrderBy(t => t.Order)
                .Select(t =>
                {
                    var assignee = t.AssigneeId.HasValue ? assignees.FirstOrDefault(a => a.Id == t.AssigneeId) : null;
                    var taskLabels = labels.Where(l => t.Labels.Any(tl => tl.LabelId == l.Id)).ToList();

                    return new ExportTaskRow(
                        t.Id, t.Title, t.Description, column.Title, t.Priority.ToString(), t.DueDate,
                        assignee?.Name, assignee?.Email.Value,
                        string.Join(", ", taskLabels.Select(l => l.Name)),
                        t.CreatedAt, t.UpdatedAt, t.Order, column.Title,
                        taskLabels.Select(l => (l.Name, l.Color.Value)).ToList());
                })
                .ToList();

            return new ExportColumnData(column.Title, columnTasks);
        }).ToList();

        var exportData = new BoardExportData(
            board.Title,
            DateTime.UtcNow,
            tasks.Count,
            board.Members.Count,
            columnsData);

        byte[] fileBytes;
        string contentType;
        string extension;

        if (request.Format == "csv")
        {
            fileBytes = csvExportService.Generate(exportData);
            contentType = "text/csv";
            extension = "csv";
        }
        else
        {
            fileBytes = pdfExportService.Generate(exportData);
            contentType = "application/pdf";
            extension = "pdf";
        }

        var slug = string.Join("-", board.Title.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var fileName = $"board-{slug}-{DateTime.UtcNow:yyyy-MM-dd}.{extension}";

        return new ExportFileResult(fileBytes, contentType, fileName);
    }
}
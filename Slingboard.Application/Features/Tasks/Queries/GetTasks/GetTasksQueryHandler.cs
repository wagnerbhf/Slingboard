using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Tasks.Queries.GetTasks;

public class GetTasksQueryHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<GetTasksQuery, List<TaskResponse>>
{
    public async ValueTask<List<TaskResponse>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        var query = context.Tasks.Include(t => t.Labels).Where(t => t.BoardId == request.BoardId);

        if (request.ColumnId.HasValue)
            query = query.Where(t => t.ColumnId == request.ColumnId);

        if (request.Priority.HasValue)
            query = query.Where(t => t.Priority == request.Priority);

        if (request.AssigneeId.HasValue)
            query = query.Where(t => t.AssigneeId == request.AssigneeId);

        if (request.DueDateFrom.HasValue)
            query = query.Where(t => t.DueDate >= request.DueDateFrom);

        if (request.DueDateTo.HasValue)
            query = query.Where(t => t.DueDate <= request.DueDateTo);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Title.Contains(request.Search) || (t.Description != null && t.Description.Contains(request.Search)));

        if (request.LabelId.HasValue)
            query = query.Where(t => t.Labels.Any(l => l.LabelId == request.LabelId));

        var tasks = await query.OrderBy(t => t.ColumnId).ThenBy(t => t.Order).ToListAsync(cancellationToken);

        var allLabelIds = tasks.SelectMany(t => t.Labels.Select(l => l.LabelId)).Distinct().ToList();
        var labelsLookup = await context.Labels
            .Where(l => allLabelIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        return tasks.Select(t => new TaskResponse(
            t.Id, t.BoardId, t.ColumnId, t.Title, t.Description,
            t.Priority.ToString(), t.DueDate, t.Order, t.AssigneeId,
            t.CreatedById, t.CreatedAt, t.UpdatedAt,
            t.Labels.Select(l => labelsLookup.TryGetValue(l.LabelId, out var label)
                ? new TaskLabelDto(label.Id, label.Name, label.Color.Value)
                : null!).Where(l => l is not null).ToList()
        )).ToList();
    }
}
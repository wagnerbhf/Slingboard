using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.Enums;
using TaskEntity = Slingboard.Domain.Entities.TaskItem;

namespace Slingboard.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler(IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    public async ValueTask<TaskResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Members)
            .Include(b => b.Columns)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        if (!board.Columns.Any(c => c.Id == request.ColumnId))
            throw new NotFoundException("Coluna não encontrada neste board.");

        var maxOrder = await context.Tasks
            .Where(t => t.ColumnId == request.ColumnId)
            .Select(t => (int?)t.Order)
            .MaxAsync(cancellationToken) ?? -1;

        var priority = Enum.Parse<TaskPriority>(request.Priority);

        var task = TaskEntity.Create(
            request.BoardId,
            request.ColumnId,
            request.Title,
            currentUser.UserId,
            maxOrder + 1,
            request.Description,
            priority,
            request.DueDate,
            request.AssigneeId);

        if (request.LabelIds is { Count: > 0 })
        {
            var validLabelIds = await context.Labels
                .Where(l => l.BoardId == request.BoardId && request.LabelIds.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);

            foreach (var labelId in validLabelIds)
                task.AddLabel(labelId);
        }

        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.NotifyTaskCreated(task.BoardId, new
        {
            taskId = task.Id,
            columnId = task.ColumnId,
            title = task.Title,
            createdByUserId = currentUser.UserId
        }, cancellationToken);

        return await MapToResponseAsync(context, task, cancellationToken);
    }

    public static async ValueTask<TaskResponse> MapToResponseAsync(IAppDbContext context, TaskEntity task, CancellationToken cancellationToken)
    {
        var labelIds = task.Labels.Select(l => l.LabelId).ToList();
        var labels = await context.Labels
            .Where(l => labelIds.Contains(l.Id))
            .Select(l => new TaskLabelDto(l.Id, l.Name, l.Color.Value))
            .ToListAsync(cancellationToken);

        return new TaskResponse(
            task.Id, task.BoardId, task.ColumnId, task.Title, task.Description,
            task.Priority.ToString(), task.DueDate, task.Order, task.AssigneeId,
            task.CreatedById, task.CreatedAt, task.UpdatedAt, labels);
    }
}
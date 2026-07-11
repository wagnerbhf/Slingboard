using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Tasks.Commands.CreateTask;

namespace Slingboard.Application.Features.Tasks.Commands.MoveTask;

public class MoveTaskCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<MoveTaskCommand, TaskResponse>
{
    public async ValueTask<TaskResponse> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await context.Tasks
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException("Task não encontrada.");

        var board = await context.Boards
            .Include(b => b.Members)
            .Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == task.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a esta task.");

        if (!board.Columns.Any(c => c.Id == request.NewColumnId))
            throw new NotFoundException("Coluna de destino não encontrada neste board.");

        var oldColumnId = task.ColumnId;
        var isSameColumn = oldColumnId == request.NewColumnId;

        var targetColumnTasks = await context.Tasks
            .Where(t => t.ColumnId == request.NewColumnId && t.Id != request.TaskId)
            .OrderBy(t => t.Order)
            .ToListAsync(cancellationToken);

        var clampedIndex = Math.Clamp(request.NewOrder, 0, targetColumnTasks.Count);
        targetColumnTasks.Insert(clampedIndex, task);

        for (var i = 0; i < targetColumnTasks.Count; i++)
        {
            if (targetColumnTasks[i].Id == task.Id)
                task.MoveTo(request.NewColumnId, i, currentUser.UserId);
            else
                targetColumnTasks[i].MoveTo(targetColumnTasks[i].ColumnId, i, currentUser.UserId);
        }

        if (!isSameColumn)
        {
            var oldColumnTasks = await context.Tasks
                .Where(t => t.ColumnId == oldColumnId && t.Id != request.TaskId)
                .OrderBy(t => t.Order)
                .ToListAsync(cancellationToken);

            for (var i = 0; i < oldColumnTasks.Count; i++)
                oldColumnTasks[i].MoveTo(oldColumnId, i, currentUser.UserId);
        }

        await context.SaveChangesAsync(cancellationToken);

        var response = await CreateTaskCommandHandler.MapToResponseAsync(context, task, cancellationToken);

        await realtimeNotifier.NotifyTaskMoved(task.BoardId, new
        {
            taskId = task.Id,
            oldColumnId,
            newColumnId = request.NewColumnId,
            newOrder = task.Order,
            movedByUserId = currentUser.UserId,
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        return response;
    }
}
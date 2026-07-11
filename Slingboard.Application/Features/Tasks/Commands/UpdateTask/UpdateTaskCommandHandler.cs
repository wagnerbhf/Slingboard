using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.Enums;

namespace Slingboard.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<UpdateTaskCommand, TaskResponse>
{
    public async ValueTask<TaskResponse> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await context.Tasks
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException("Task não encontrada.");

        var board = await context.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == task.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a esta task.");

        var priority = Enum.Parse<TaskPriority>(request.Priority);
        task.UpdateDetails(request.Title, request.Description, priority, request.DueDate);

        if (request.LabelIds is not null)
        {
            var currentLabelIds = task.Labels.Select(l => l.LabelId).ToList();
            foreach (var labelId in currentLabelIds.Except(request.LabelIds))
                task.RemoveLabel(labelId);

            var validNewLabelIds = await context.Labels
                .Where(l => l.BoardId == task.BoardId && request.LabelIds.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);

            foreach (var labelId in validNewLabelIds.Except(currentLabelIds))
                task.AddLabel(labelId);
        }

        await context.SaveChangesAsync(cancellationToken);

        var response = await Commands.CreateTask.CreateTaskCommandHandler.MapToResponseAsync(context, task, cancellationToken);

        await realtimeNotifier.NotifyTaskUpdated(task.BoardId, new
        {
            taskId = task.Id,
            title = task.Title,
            priority = task.Priority.ToString(),
            updatedByUserId = currentUser.UserId
        }, cancellationToken);

        return response;
    }
}
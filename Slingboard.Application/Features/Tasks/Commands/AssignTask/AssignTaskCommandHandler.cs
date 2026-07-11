using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Tasks.Commands.AssignTask;

public class AssignTaskCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<AssignTaskCommand, TaskResponse>
{
    public async ValueTask<TaskResponse> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
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

        if (request.AssigneeId.HasValue && !board.IsMember(request.AssigneeId.Value))
            throw new Domain.Exceptions.BusinessRuleViolationException("O assignee deve ser membro do board.");

        task.AssignTo(request.AssigneeId);
        await context.SaveChangesAsync(cancellationToken);

        var response = await Commands.CreateTask.CreateTaskCommandHandler.MapToResponseAsync(context, task, cancellationToken);

        await realtimeNotifier.NotifyTaskAssigned(task.BoardId, new
        {
            taskId = task.Id,
            assigneeId = task.AssigneeId,
            assignedByUserId = currentUser.UserId
        }, cancellationToken);

        return response;
    }
}
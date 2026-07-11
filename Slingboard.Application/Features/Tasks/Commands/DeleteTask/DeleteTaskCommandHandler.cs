using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<DeleteTaskCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await context.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new NotFoundException("Task não encontrada.");

        var board = await context.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == task.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a esta task.");

        var boardId = task.BoardId;
        var taskId = task.Id;

        context.Tasks.Remove(task);
        await context.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.NotifyTaskDeleted(boardId, new
        {
            taskId,
            deletedByUserId = currentUser.UserId
        }, cancellationToken);

        return Unit.Value;
    }
}
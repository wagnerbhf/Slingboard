using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Tasks.Queries.GetTaskById;

public class GetTaskByIdQueryHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<GetTaskByIdQuery, TaskResponse>
{
    public async ValueTask<TaskResponse> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
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

        return await Commands.CreateTask.CreateTaskCommandHandler.MapToResponseAsync(context, task, cancellationToken);
    }
}
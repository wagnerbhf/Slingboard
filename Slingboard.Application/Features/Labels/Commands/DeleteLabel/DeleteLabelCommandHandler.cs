using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Labels.Commands.DeleteLabel;

public class DeleteLabelCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<DeleteLabelCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteLabelCommand request, CancellationToken cancellationToken)
    {
        var label = await context.Labels.FirstOrDefaultAsync(l => l.Id == request.LabelId, cancellationToken)
            ?? throw new NotFoundException("Label não encontrada.");

        var board = await context.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == label.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        var boardId = board.Id;
        var labelId = label.Id;

        var taskLabels = context.TaskLabels.Where(tl => tl.LabelId == label.Id);
        context.TaskLabels.RemoveRange(taskLabels);

        context.Labels.Remove(label);
        await context.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.NotifyLabelDeleted(boardId, new
        {
            labelId,
            deletedByUserId = currentUser.UserId
        }, cancellationToken);

        return Unit.Value;
    }
}
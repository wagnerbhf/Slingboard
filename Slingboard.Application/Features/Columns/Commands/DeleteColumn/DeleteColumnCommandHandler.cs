using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Columns.Commands.DeleteColumn;

public class DeleteColumnCommandHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<DeleteColumnCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        var column = await context.Columns.FirstOrDefaultAsync(c => c.Id == request.ColumnId, cancellationToken)
            ?? throw new NotFoundException("Coluna não encontrada.");

        var board = await context.Boards
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == column.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        if (board.Columns.Count <= 1)
            throw new Domain.Exceptions.BusinessRuleViolationException("O board deve ter pelo menos uma coluna.");

        var targetColumnId = request.MoveTasksToColumnId
            ?? board.Columns.First(c => c.Id != request.ColumnId).Id;

        var tasksToMove = await context.Tasks.Where(t => t.ColumnId == request.ColumnId).ToListAsync(cancellationToken);
        var maxOrder = await context.Tasks.Where(t => t.ColumnId == targetColumnId)
            .Select(t => (int?)t.Order).MaxAsync(cancellationToken) ?? -1;

        foreach (var task in tasksToMove)
        {
            maxOrder++;
            task.MoveTo(targetColumnId, maxOrder, currentUser.UserId);
        }

        board.RemoveColumn(request.ColumnId);

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
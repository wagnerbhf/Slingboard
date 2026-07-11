using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Boards;

namespace Slingboard.Application.Features.Columns.Commands.UpdateColumn;

public class UpdateColumnCommandHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<UpdateColumnCommand, ColumnDto>
{
    public async ValueTask<ColumnDto> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        var column = await context.Columns.FirstOrDefaultAsync(c => c.Id == request.ColumnId, cancellationToken)
            ?? throw new NotFoundException("Coluna não encontrada.");

        var board = await context.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == column.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        column.UpdateDetails(request.Title, request.Limit);

        await context.SaveChangesAsync(cancellationToken);

        return new ColumnDto(column.Id, column.Title, column.Order, column.Limit);
    }
}
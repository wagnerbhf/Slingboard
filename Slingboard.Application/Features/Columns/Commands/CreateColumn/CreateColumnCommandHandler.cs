using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Application.Features.Boards;

namespace Slingboard.Application.Features.Columns.Commands.CreateColumn;

public class CreateColumnCommandHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<CreateColumnCommand, ColumnDto>
{
    public async ValueTask<ColumnDto> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        var column = board.AddColumn(request.Title, request.Limit);

        context.Columns.Add(column);

        await context.SaveChangesAsync(cancellationToken);

        return new ColumnDto(column.Id, column.Title, column.Order, column.Limit);
    }
}
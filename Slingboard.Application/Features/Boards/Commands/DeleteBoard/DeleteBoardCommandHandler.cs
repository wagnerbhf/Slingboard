using Mediator;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Boards.Commands.DeleteBoard;

public class DeleteBoardCommandHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<DeleteBoardCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = context.Boards.FirstOrDefault(b => b.Id == request.BoardId)
            ?? throw new NotFoundException("Board não encontrado.");

        if (board.OwnerId != currentUser.UserId)
            throw new ForbiddenException("Apenas o Owner pode excluir o board.");

        context.Boards.Remove(board);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Boards.Queries.GetBoardById;

public class GetBoardByIdQueryHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<GetBoardByIdQuery, BoardDetailResponse>
{
    public async ValueTask<BoardDetailResponse> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        var memberUserIds = board.Members.Select(m => m.UserId).ToList();
        var users = await context.Users
            .Where(u => memberUserIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var members = board.Members.Select(m =>
        {
            var user = users.First(u => u.Id == m.UserId);
            return new Application.Features.Boards.MemberDto(user.Id, user.Name, user.Email.Value, m.Role.ToString());
        }).ToList();

        return Commands.CreateBoard.CreateBoardCommandHandler.MapToResponse(board, members);
    }
}
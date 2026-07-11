using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Features.Boards.Commands.UpdateBoard;

public class UpdateBoardCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<UpdateBoardCommand, BoardDetailResponse>
{
    public async ValueTask<BoardDetailResponse> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Columns)
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        var backgroundColor = request.BackgroundColor is not null ? HexColor.Create(request.BackgroundColor) : null;
        board.UpdateDetails(request.Title, request.Description, backgroundColor);

        await context.SaveChangesAsync(cancellationToken);

        var memberUserIds = board.Members.Select(m => m.UserId).ToList();
        var users = await context.Users.Where(u => memberUserIds.Contains(u.Id)).ToListAsync(cancellationToken);

        var members = board.Members.Select(m =>
        {
            var user = users.First(u => u.Id == m.UserId);
            return new MemberDto(user.Id, user.Name, user.Email.Value, m.Role.ToString());
        }).ToList();

        var response = Commands.CreateBoard.CreateBoardCommandHandler.MapToResponse(board, members);

        await realtimeNotifier.NotifyBoardUpdated(board.Id, new
        {
            boardId = board.Id,
            title = board.Title,
            updatedByUserId = currentUser.UserId
        }, cancellationToken);

        return response;
    }
}
using Mediator;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.Entities;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Features.Boards.Commands.CreateBoard;

public class CreateBoardCommandHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<CreateBoardCommand, BoardDetailResponse>
{
    public async ValueTask<BoardDetailResponse> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var backgroundColor = request.BackgroundColor is not null ? HexColor.Create(request.BackgroundColor) : null;

        var board = Board.Create(request.Title, currentUser.UserId, request.Description, backgroundColor);

        context.Boards.Add(board);
        await context.SaveChangesAsync(cancellationToken);

        return MapToResponse(board, []);
    }

    public static BoardDetailResponse MapToResponse(Board board, IReadOnlyCollection<MemberDto> members) => new(
        board.Id,
        board.Title,
        board.Description,
        board.OwnerId,
        board.BackgroundColor?.Value,
        board.IsPublic,
        board.CreatedAt,
        board.UpdatedAt,
        board.Columns.OrderBy(c => c.Order).Select(c => new ColumnDto(c.Id, c.Title, c.Order, c.Limit)).ToList(),
        members);
}
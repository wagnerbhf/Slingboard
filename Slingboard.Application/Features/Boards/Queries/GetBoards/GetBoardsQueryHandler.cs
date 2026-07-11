using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Boards.Queries.GetBoards;

public class GetBoardsQueryHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<GetBoardsQuery, List<BoardSummaryResponse>>
{
    public async ValueTask<List<BoardSummaryResponse>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        var query = context.Boards
            .Include(b => b.Members)
            .Where(b => b.Members.Any(m => m.UserId == userId));

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(b => b.Title.Contains(request.Search));

        var boards = await query.OrderByDescending(b => b.UpdatedAt).ToListAsync(cancellationToken);
        var boardIds = boards.Select(b => b.Id).ToList();

        var taskCounts = await context.Tasks
            .Where(t => boardIds.Contains(t.BoardId))
            .GroupBy(t => t.BoardId)
            .Select(g => new { BoardId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return boards.Select(b => new BoardSummaryResponse(
            b.Id,
            b.Title,
            b.Description,
            b.BackgroundColor?.Value,
            taskCounts.FirstOrDefault(tc => tc.BoardId == b.Id)?.Count ?? 0,
            b.Members.Count,
            b.UpdatedAt
        )).ToList();
    }
}
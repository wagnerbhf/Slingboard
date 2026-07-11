using Mediator;

namespace Slingboard.Application.Features.Boards.Queries.GetBoards;

public record GetBoardsQuery(string? Search) : IRequest<List<BoardSummaryResponse>>;
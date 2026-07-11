using Mediator;

namespace Slingboard.Application.Features.Boards.Queries.GetBoardById;

public record GetBoardByIdQuery(Guid BoardId) : IRequest<BoardDetailResponse>;
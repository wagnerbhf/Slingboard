using Mediator;

namespace Slingboard.Application.Features.Boards.Commands.UpdateBoard;

public record UpdateBoardCommand(Guid BoardId, string Title, string? Description, string? BackgroundColor) : IRequest<BoardDetailResponse>;
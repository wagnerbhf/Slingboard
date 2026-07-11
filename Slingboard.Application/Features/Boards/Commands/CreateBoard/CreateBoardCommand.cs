using Mediator;

namespace Slingboard.Application.Features.Boards.Commands.CreateBoard;

public record CreateBoardCommand(string Title, string? Description, string? BackgroundColor) : IRequest<BoardDetailResponse>;
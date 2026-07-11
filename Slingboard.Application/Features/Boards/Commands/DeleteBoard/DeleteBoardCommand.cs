using Mediator;

namespace Slingboard.Application.Features.Boards.Commands.DeleteBoard;

public record DeleteBoardCommand(Guid BoardId) : IRequest<Unit>;
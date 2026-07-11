using Mediator;

namespace Slingboard.Application.Features.Tasks.Commands.MoveTask;

public record MoveTaskCommand(Guid TaskId, Guid NewColumnId, int NewOrder) : IRequest<TaskResponse>;
using Mediator;

namespace Slingboard.Application.Features.Tasks.Commands.AssignTask;

public record AssignTaskCommand(Guid TaskId, Guid? AssigneeId) : IRequest<TaskResponse>;
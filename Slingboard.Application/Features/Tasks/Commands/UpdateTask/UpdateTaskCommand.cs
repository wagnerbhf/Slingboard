using Mediator;

namespace Slingboard.Application.Features.Tasks.Commands.UpdateTask;

public record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    string? Description,
    string Priority,
    DateTime? DueDate,
    List<Guid>? LabelIds) : IRequest<TaskResponse>;
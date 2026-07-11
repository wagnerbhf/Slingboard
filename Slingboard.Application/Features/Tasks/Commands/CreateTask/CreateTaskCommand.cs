using Mediator;

namespace Slingboard.Application.Features.Tasks.Commands.CreateTask;

public record CreateTaskCommand(
    Guid BoardId,
    Guid ColumnId,
    string Title,
    string? Description,
    string Priority,
    DateTime? DueDate,
    List<Guid>? LabelIds,
    Guid? AssigneeId) : IRequest<TaskResponse>;
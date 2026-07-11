using Mediator;
using Slingboard.Domain.Enums;

namespace Slingboard.Application.Features.Tasks.Queries.GetTasks;

public record GetTasksQuery(
    Guid BoardId,
    Guid? ColumnId,
    TaskPriority? Priority,
    Guid? LabelId,
    Guid? AssigneeId,
    DateTime? DueDateFrom,
    DateTime? DueDateTo,
    string? Search) : IRequest<List<TaskResponse>>;
namespace Slingboard.Application.Features.Tasks;

public record TaskLabelDto(Guid Id, string Name, string Color);

public record TaskResponse(
    Guid Id,
    Guid BoardId,
    Guid ColumnId,
    string Title,
    string? Description,
    string Priority,
    DateTime? DueDate,
    int Order,
    Guid? AssigneeId,
    Guid CreatedById,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<TaskLabelDto> Labels);
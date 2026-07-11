using Mediator;

namespace Slingboard.Application.Features.Columns.Commands.DeleteColumn;

public record DeleteColumnCommand(Guid ColumnId, Guid? MoveTasksToColumnId) : IRequest<Unit>;
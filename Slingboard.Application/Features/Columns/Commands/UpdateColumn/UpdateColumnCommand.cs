using Mediator;

namespace Slingboard.Application.Features.Columns.Commands.UpdateColumn;

public record UpdateColumnCommand(Guid ColumnId, string Title, int? Limit) : IRequest<Boards.ColumnDto>;
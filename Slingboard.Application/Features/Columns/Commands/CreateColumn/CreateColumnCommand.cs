using Mediator;
using Slingboard.Application.Features.Boards;

namespace Slingboard.Application.Features.Columns.Commands.CreateColumn;

public record CreateColumnCommand(Guid BoardId, string Title, int? Limit) : IRequest<ColumnDto>;
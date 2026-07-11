using Mediator;

namespace Slingboard.Application.Features.Exports.Queries.ExportBoard;

public record ExportBoardQuery(
    Guid BoardId,
    string Format,
    bool IncludeCompleted,
    DateTime? DateFrom,
    DateTime? DateTo) : IRequest<ExportFileResult>;
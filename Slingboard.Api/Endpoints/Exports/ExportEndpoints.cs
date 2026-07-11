using Mediator;
using Slingboard.Application.Features.Exports.Queries.ExportBoard;

namespace Slingboard.Api.Endpoints.Exports;

public static class ExportEndpoints
{
    public static void MapExportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/boards/{boardId:guid}/export", async (
            Guid boardId, string format, bool includeCompleted, DateTime? dateFrom, DateTime? dateTo,
            ISender sender, CancellationToken ct) =>
        {
            var query = new ExportBoardQuery(boardId, format, includeCompleted, dateFrom, dateTo);
            var result = await sender.Send(query, ct);
            return Results.File(result.Content, result.ContentType, result.FileName);
        })
        .WithTags("Exports").WithName("ExportBoard").RequireAuthorization()
        .Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);
    }
}
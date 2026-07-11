using Mediator;
using Slingboard.Application.Features.Columns.Commands.CreateColumn;
using Slingboard.Application.Features.Columns.Commands.DeleteColumn;
using Slingboard.Application.Features.Columns.Commands.UpdateColumn;

namespace Slingboard.Api.Endpoints.Columns;

public static class ColumnEndpoints
{
    public static void MapColumnEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/boards/{boardId:guid}/columns", async (Guid boardId, CreateColumnRequest body, ISender sender, CancellationToken ct) =>
        {
            var command = new CreateColumnCommand(boardId, body.Title, body.Limit);
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/v1/columns/{result.Id}", result);
        })
        .WithTags("Columns")
        .WithName("CreateColumn")
        .RequireAuthorization()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        app.MapPut("/api/v1/columns/{columnId:guid}", async (Guid columnId, UpdateColumnRequest body, ISender sender, CancellationToken ct) =>
        {
            var command = new UpdateColumnCommand(columnId, body.Title, body.Limit);
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .WithTags("Columns")
        .WithName("UpdateColumn")
        .RequireAuthorization()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        app.MapDelete("/api/v1/columns/{columnId:guid}", async (Guid columnId, Guid? moveTasksToColumnId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteColumnCommand(columnId, moveTasksToColumnId), ct);
            return Results.NoContent();
        })
        .WithTags("Columns")
        .WithName("DeleteColumn")
        .RequireAuthorization()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public record CreateColumnRequest(string Title, int? Limit);
public record UpdateColumnRequest(string Title, int? Limit);
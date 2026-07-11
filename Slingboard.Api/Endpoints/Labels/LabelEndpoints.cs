using Mediator;
using Slingboard.Application.Features.Labels.Commands.CreateLabel;
using Slingboard.Application.Features.Labels.Commands.DeleteLabel;
using Slingboard.Application.Features.Labels.Commands.UpdateLabel;
using Slingboard.Application.Features.Labels.Queries.GetLabels;

namespace Slingboard.Api.Endpoints.Labels;

public static class LabelEndpoints
{
    public static void MapLabelEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/boards/{boardId:guid}/labels", async (Guid boardId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetLabelsQuery(boardId), ct);
            return Results.Ok(result);
        })
        .WithTags("Labels").WithName("GetLabels").RequireAuthorization();

        app.MapPost("/api/v1/boards/{boardId:guid}/labels", async (Guid boardId, CreateLabelRequest body, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateLabelCommand(boardId, body.Name, body.Color), ct);
            return Results.Created($"/api/v1/labels/{result.Id}", result);
        })
        .WithTags("Labels").WithName("CreateLabel").RequireAuthorization()
        .ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);

        app.MapPut("/api/v1/labels/{labelId:guid}", async (Guid labelId, UpdateLabelRequest body, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateLabelCommand(labelId, body.Name, body.Color), ct);
            return Results.Ok(result);
        })
        .WithTags("Labels").WithName("UpdateLabel").RequireAuthorization()
        .ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);

        app.MapDelete("/api/v1/labels/{labelId:guid}", async (Guid labelId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteLabelCommand(labelId), ct);
            return Results.NoContent();
        })
        .WithTags("Labels").WithName("DeleteLabel").RequireAuthorization()
        .Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);
    }
}

public record CreateLabelRequest(string Name, string Color);
public record UpdateLabelRequest(string Name, string Color);
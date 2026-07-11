using Mediator;
using Slingboard.Application.Features.Boards.Commands.AddMember;
using Slingboard.Application.Features.Boards.Commands.CreateBoard;
using Slingboard.Application.Features.Boards.Commands.DeleteBoard;
using Slingboard.Application.Features.Boards.Commands.UpdateBoard;
using Slingboard.Application.Features.Boards.Queries.GetBoardById;
using Slingboard.Application.Features.Boards.Queries.GetBoards;

namespace Slingboard.Api.Endpoints.Boards;

public static class BoardEndpoints
{
    public static void MapBoardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/boards").WithTags("Boards").RequireAuthorization();

        group.MapGet("/", async (string? search, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBoardsQuery(search), ct);
            return Results.Ok(result);
        })
        .WithName("GetBoards");

        group.MapPost("/", async (CreateBoardCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/v1/boards/{result.Id}", result);
        })
        .WithName("CreateBoard")
        .ProducesValidationProblem();

        group.MapGet("/{boardId:guid}", async (Guid boardId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBoardByIdQuery(boardId), ct);
            return Results.Ok(result);
        })
        .WithName("GetBoardById")
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{boardId:guid}", async (Guid boardId, UpdateBoardRequest body, ISender sender, CancellationToken ct) =>
        {
            var command = new UpdateBoardCommand(boardId, body.Title, body.Description, body.BackgroundColor);
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("UpdateBoard")
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/{boardId:guid}", async (Guid boardId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteBoardCommand(boardId), ct);
            return Results.NoContent();
        })
        .WithName("DeleteBoard")
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/{boardId:guid}/members", async (Guid boardId, AddMemberRequest body, ISender sender, CancellationToken ct) =>
        {
            var command = new AddMemberCommand(boardId, body.UserId, body.Role);
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/v1/boards/{boardId}/members/{result.UserId}", result);
        })
        .WithName("AddMember")
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public record UpdateBoardRequest(string Title, string? Description, string? BackgroundColor);
public record AddMemberRequest(Guid UserId, string Role);
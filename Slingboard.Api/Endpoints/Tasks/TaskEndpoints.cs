using Mediator;
using Slingboard.Application.Features.Tasks.Commands.AssignTask;
using Slingboard.Application.Features.Tasks.Commands.CreateTask;
using Slingboard.Application.Features.Tasks.Commands.DeleteTask;
using Slingboard.Application.Features.Tasks.Commands.MoveTask;
using Slingboard.Application.Features.Tasks.Commands.UpdateTask;
using Slingboard.Application.Features.Tasks.Queries.GetTaskById;
using Slingboard.Application.Features.Tasks.Queries.GetTasks;
using Slingboard.Domain.Enums;

namespace Slingboard.Api.Endpoints.Tasks;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/boards/{boardId:guid}/tasks", async (
            Guid boardId, Guid? columnId, TaskPriority? priority, Guid? labelId,
            Guid? assigneeId, DateTime? dueDateFrom, DateTime? dueDateTo, string? search,
            ISender sender, CancellationToken ct) =>
        {
            var query = new GetTasksQuery(boardId, columnId, priority, labelId, assigneeId, dueDateFrom, dueDateTo, search);
            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .WithTags("Tasks").WithName("GetTasks").RequireAuthorization();

        app.MapPost("/api/v1/boards/{boardId:guid}/tasks", async (Guid boardId, CreateTaskRequest body, ISender sender, CancellationToken ct) =>
        {
            var command = new CreateTaskCommand(boardId, body.ColumnId, body.Title, body.Description, body.Priority, body.DueDate, body.LabelIds, body.AssigneeId);
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/v1/tasks/{result.Id}", result);
        })
        .WithTags("Tasks").WithName("CreateTask").RequireAuthorization()
        .ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);

        app.MapGet("/api/v1/tasks/{taskId:guid}", async (Guid taskId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTaskByIdQuery(taskId), ct);
            return Results.Ok(result);
        })
        .WithTags("Tasks").WithName("GetTaskById").RequireAuthorization()
        .Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);

        app.MapPut("/api/v1/tasks/{taskId:guid}", async (Guid taskId, UpdateTaskRequest body, ISender sender, CancellationToken ct) =>
        {
            var command = new UpdateTaskCommand(taskId, body.Title, body.Description, body.Priority, body.DueDate, body.LabelIds);
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .WithTags("Tasks").WithName("UpdateTask").RequireAuthorization()
        .ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/api/v1/tasks/{taskId:guid}/assign", async (Guid taskId, AssignTaskRequest body, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AssignTaskCommand(taskId, body.AssigneeId), ct);
            return Results.Ok(result);
        })
        .WithTags("Tasks").WithName("AssignTask").RequireAuthorization()
        .Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);

        app.MapDelete("/api/v1/tasks/{taskId:guid}", async (Guid taskId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteTaskCommand(taskId), ct);
            return Results.NoContent();
        })
        .WithTags("Tasks").WithName("DeleteTask").RequireAuthorization()
        .Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);

        app.MapPatch("/api/v1/tasks/{taskId:guid}/move", async (Guid taskId, MoveTaskRequest body, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new MoveTaskCommand(taskId, body.NewColumnId, body.NewOrder), ct);
            return Results.Ok(result);
        })
        .WithTags("Tasks").WithName("MoveTask").RequireAuthorization()
        .ProducesValidationProblem().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status403Forbidden);
    }
}

public record CreateTaskRequest(Guid ColumnId, string Title, string? Description, string Priority, DateTime? DueDate, List<Guid>? LabelIds, Guid? AssigneeId);
public record UpdateTaskRequest(string Title, string? Description, string Priority, DateTime? DueDate, List<Guid>? LabelIds);
public record AssignTaskRequest(Guid? AssigneeId);
public record MoveTaskRequest(Guid NewColumnId, int NewOrder);
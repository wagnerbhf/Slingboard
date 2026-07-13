using Mediator;
using Slingboard.Application.Features.Users.Queries.SearchUsers;

namespace Slingboard.Api.Endpoints.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/users", async (string? search, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SearchUsersQuery(search), ct);
            return Results.Ok(result);
        })
        .WithTags("Users")
        .WithName("SearchUsers")
        .RequireAuthorization();
    }
}
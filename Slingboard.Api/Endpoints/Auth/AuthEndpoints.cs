using Mediator;
using Slingboard.Application.Features.Auth.Login;
using Slingboard.Application.Features.Auth.RefreshToken;
using Slingboard.Application.Features.Auth.Register;

namespace Slingboard.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/v1/users/{result.Id}", result);
        })
        .WithName("Register")
        .Produces<RegisterResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapPost("/login", async (LoginCommand command, ISender sender, HttpContext http, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            http.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Results.Ok(new { accessToken = result.AccessToken, expiresIn = result.ExpiresIn });
        })
        .WithName("Login")
        .RequireRateLimiting("login")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", async (HttpContext http, ISender sender, CancellationToken ct) =>
        {
            var rawToken = http.Request.Cookies["refreshToken"];
            var command = new RefreshTokenCommand(rawToken ?? string.Empty);

            var result = await sender.Send(command, ct);

            http.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Results.Ok(new { accessToken = result.AccessToken, expiresIn = result.ExpiresIn });
        })
        .WithName("RefreshToken")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", (HttpContext http) =>
        {
            http.Response.Cookies.Delete("refreshToken");
            return Results.NoContent();
        })
        .WithName("Logout")
        .Produces(StatusCodes.Status204NoContent);
    }
}
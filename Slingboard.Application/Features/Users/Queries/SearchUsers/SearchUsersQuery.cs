using Mediator;

namespace Slingboard.Application.Features.Users.Queries.SearchUsers;

public record SearchUsersQuery(string? Search) : IRequest<List<UserSummaryResponse>>;
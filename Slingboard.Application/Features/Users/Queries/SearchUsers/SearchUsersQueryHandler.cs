using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Users.Queries.SearchUsers;

public class SearchUsersQueryHandler(IAppDbContext context) : IRequestHandler<SearchUsersQuery, List<UserSummaryResponse>>
{
    private const int MaxResults = 20;

    public async ValueTask<List<UserSummaryResponse>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var activeUsers = await context.Users
            .Where(u => u.IsActive)
            .Select(u => new UserSummaryResponse(u.Id, u.Name, u.Email.Value))
            .ToListAsync(cancellationToken);

        IEnumerable<UserSummaryResponse> filtered = activeUsers;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            filtered = activeUsers.Where(u =>
                u.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return filtered
            .OrderBy(u => u.Name)
            .Take(MaxResults)
            .ToList();
    }
}
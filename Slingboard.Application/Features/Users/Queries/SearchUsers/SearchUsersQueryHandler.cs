using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Users.Queries.SearchUsers;

public class SearchUsersQueryHandler(IAppDbContext context) : IRequestHandler<SearchUsersQuery, List<UserSummaryResponse>>
{
    private const int MaxResults = 20;

    public async ValueTask<List<UserSummaryResponse>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Users.Where(u => u.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                u.Email.Value.Contains(term, StringComparison.CurrentCultureIgnoreCase));
        }

        return await query
            .OrderBy(u => u.Name)
            .Take(MaxResults)
            .Select(u => new UserSummaryResponse(u.Id, u.Name, u.Email.Value))
            .ToListAsync(cancellationToken);
    }
}
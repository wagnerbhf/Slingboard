using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Application.Features.Labels.Queries.GetLabels;

public class GetLabelsQueryHandler(IAppDbContext context, ICurrentUserService currentUser) : IRequestHandler<GetLabelsQuery, List<LabelResponse>>
{
    public async ValueTask<List<LabelResponse>> Handle(GetLabelsQuery request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        return await context.Labels
            .Where(l => l.BoardId == request.BoardId)
            .Select(l => new LabelResponse(l.Id, l.BoardId, l.Name, l.Color.Value))
            .ToListAsync(cancellationToken);
    }
}
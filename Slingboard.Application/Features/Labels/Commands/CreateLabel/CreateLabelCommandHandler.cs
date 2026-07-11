using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Features.Labels.Commands.CreateLabel;

public class CreateLabelCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<CreateLabelCommand, LabelResponse>
{
    public async ValueTask<LabelResponse> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Members)
            .Include(b => b.Labels)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        var color = HexColor.Create(request.Color);
        var label = board.AddLabel(request.Name, color);

        context.Labels.Add(label);
        await context.SaveChangesAsync(cancellationToken);

        var response = new LabelResponse(label.Id, label.BoardId, label.Name, label.Color.Value);

        await realtimeNotifier.NotifyLabelCreated(board.Id, new
        {
            labelId = label.Id,
            name = label.Name,
            color = label.Color.Value,
            createdByUserId = currentUser.UserId
        }, cancellationToken);

        return response;
    }
}
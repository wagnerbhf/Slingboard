using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Features.Labels.Commands.UpdateLabel;

public class UpdateLabelCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<UpdateLabelCommand, LabelResponse>
{
    public async ValueTask<LabelResponse> Handle(UpdateLabelCommand request, CancellationToken cancellationToken)
    {
        var label = await context.Labels.FirstOrDefaultAsync(l => l.Id == request.LabelId, cancellationToken)
            ?? throw new NotFoundException("Label não encontrada.");

        var board = await context.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == label.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        if (!board.IsMember(currentUser.UserId))
            throw new ForbiddenException("Você não tem acesso a este board.");

        var color = HexColor.Create(request.Color);
        label.Update(request.Name, color);

        await context.SaveChangesAsync(cancellationToken);

        var response = new LabelResponse(label.Id, label.BoardId, label.Name, label.Color.Value);

        await realtimeNotifier.NotifyLabelUpdated(board.Id, new
        {
            labelId = label.Id,
            name = label.Name,
            color = label.Color.Value,
            updatedByUserId = currentUser.UserId
        }, cancellationToken);

        return response;
    }
}
using Mediator;
using Microsoft.EntityFrameworkCore;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.Enums;

namespace Slingboard.Application.Features.Boards.Commands.AddMember;

public class AddMemberCommandHandler(
    IAppDbContext context,
    ICurrentUserService currentUser,
    IRealtimeNotifier realtimeNotifier) : IRequestHandler<AddMemberCommand, MemberDto>
{
    public async ValueTask<MemberDto> Handle(AddMemberCommand request, CancellationToken cancellationToken)
    {
        var board = await context.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new NotFoundException("Board não encontrado.");

        var currentMember = board.Members.FirstOrDefault(m => m.UserId == currentUser.UserId);
        if (currentMember is null || (currentMember.Role != BoardMemberRole.Owner && currentMember.Role != BoardMemberRole.Admin))
            throw new ForbiddenException("Apenas Owner ou Admin podem adicionar membros.");

        var userToAdd = await context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var role = Enum.Parse<BoardMemberRole>(request.Role);
        board.AddMember(userToAdd.Id, role);

        await context.SaveChangesAsync(cancellationToken);

        var memberDto = new MemberDto(userToAdd.Id, userToAdd.Name, userToAdd.Email.Value, role.ToString());

        await realtimeNotifier.NotifyMemberJoined(board.Id, new
        {
            boardId = board.Id,
            userId = userToAdd.Id,
            name = userToAdd.Name,
            role = role.ToString(),
            addedByUserId = currentUser.UserId
        }, cancellationToken);

        return memberDto;
    }
}
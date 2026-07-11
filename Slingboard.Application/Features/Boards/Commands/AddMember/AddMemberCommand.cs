using Mediator;

namespace Slingboard.Application.Features.Boards.Commands.AddMember;

public record AddMemberCommand(Guid BoardId, Guid UserId, string Role) : IRequest<MemberDto>;
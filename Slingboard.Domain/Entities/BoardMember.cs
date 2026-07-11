using Slingboard.Domain.Enums;

namespace Slingboard.Domain.Entities;

public class BoardMember
{
    public Guid BoardId { get; private set; }
    public Guid UserId { get; private set; }
    public BoardMemberRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private BoardMember() { }

    internal static BoardMember Create(Guid boardId, Guid userId, BoardMemberRole role)
        => new()
        {
            BoardId = boardId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

    internal void ChangeRole(BoardMemberRole newRole) => Role = newRole;
}
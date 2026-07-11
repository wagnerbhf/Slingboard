using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Api.Hubs;

[Authorize]
public class KanbanHub(IAppDbContext context) : Hub
{
    public async Task JoinBoard(Guid boardId)
    {
        var userId = GetUserId();
        var isMember = context.Boards.Any(b => b.Id == boardId && b.Members.Any(m => m.UserId == userId));

        if (!isMember)
            throw new HubException("Você não tem acesso a este board.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(boardId));
    }

    public async Task LeaveBoard(Guid boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(boardId));
    }

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    public static string GroupName(Guid boardId) => $"board-{boardId}";
}
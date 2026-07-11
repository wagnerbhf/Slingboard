using Microsoft.AspNetCore.SignalR;
using Slingboard.Api.Hubs;
using Slingboard.Application.Common.Interfaces;

namespace Slingboard.Api.Realtime;

public class SignalRNotifier(IHubContext<KanbanHub> hubContext) : IRealtimeNotifier
{
    public Task NotifyTaskCreated(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("TaskCreated", payload, cancellationToken);

    public Task NotifyTaskUpdated(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("TaskUpdated", payload, cancellationToken);

    public Task NotifyTaskMoved(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("TaskMoved", payload, cancellationToken);

    public Task NotifyTaskDeleted(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("TaskDeleted", payload, cancellationToken);

    public Task NotifyTaskAssigned(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("TaskAssigned", payload, cancellationToken);

    public Task NotifyBoardUpdated(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("BoardUpdated", payload, cancellationToken);

    public Task NotifyMemberJoined(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("MemberJoined", payload, cancellationToken);

    public Task NotifyLabelCreated(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("LabelCreated", payload, cancellationToken);

    public Task NotifyLabelUpdated(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("LabelUpdated", payload, cancellationToken);

    public Task NotifyLabelDeleted(Guid boardId, object payload, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(KanbanHub.GroupName(boardId)).SendAsync("LabelDeleted", payload, cancellationToken);
}
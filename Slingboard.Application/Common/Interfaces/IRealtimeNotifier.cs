namespace Slingboard.Application.Common.Interfaces;

public interface IRealtimeNotifier
{
    Task NotifyTaskCreated(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyTaskUpdated(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyTaskMoved(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyTaskDeleted(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyTaskAssigned(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyBoardUpdated(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyMemberJoined(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyLabelCreated(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyLabelUpdated(Guid boardId, object payload, CancellationToken cancellationToken = default);
    Task NotifyLabelDeleted(Guid boardId, object payload, CancellationToken cancellationToken = default);
}
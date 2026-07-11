using Slingboard.Domain.Common;

namespace Slingboard.Domain.Events;

public record TaskMovedDomainEvent(
    Guid TaskId,
    Guid OldColumnId,
    Guid NewColumnId,
    int NewOrder,
    Guid MovedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.Now;
}
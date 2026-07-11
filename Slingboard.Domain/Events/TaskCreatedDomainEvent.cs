using Slingboard.Domain.Common;

namespace Slingboard.Domain.Events;

public record TaskCreatedDomainEvent(Guid TaskId, Guid BoardId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.Now;
}
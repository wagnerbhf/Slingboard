namespace Slingboard.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
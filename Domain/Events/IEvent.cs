namespace SimpleLedger.Domain.Events;

public interface IEvent
{
    Guid AggregateId { get; }
    int Version { get; }
    DateTime Timestamp { get; }
}
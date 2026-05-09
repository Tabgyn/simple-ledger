using SimpleLedger.Domain.Events;

namespace SimpleLedger.Infrastructure;

public interface IEventStore
{
    Task AppendEventsAsync(Guid aggregateId, IEnumerable<IEvent> events, int expectedVersion);
    Task<IReadOnlyList<IEvent>> GetEventsAsync(Guid aggregateId);
    Task<int> GetCurrentVersionAsync(Guid aggregateId);
}
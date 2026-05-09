using System.Collections.Concurrent;
using SimpleLedger.Domain.Events;

namespace SimpleLedger.Infrastructure;

public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<Guid, List<IEvent>> _eventStreams = new();

    public Task AppendEventsAsync(Guid aggregateId, IEnumerable<IEvent> events, int expectedVersion)
    {
        var eventList = events.ToList();
        if (!eventList.Any()) return Task.CompletedTask;

        _eventStreams.AddOrUpdate(aggregateId,
            _ => { if (expectedVersion != 0) throw new ConcurrencyException(); return eventList; },
            (_, existingEvents) =>
            {
                if (existingEvents.Count != expectedVersion)
                    throw new ConcurrencyException();
                existingEvents.AddRange(eventList);
                return existingEvents;
            });

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IEvent>> GetEventsAsync(Guid aggregateId)
    {
        return Task.FromResult(_eventStreams.TryGetValue(aggregateId, out var events)
            ? events.AsReadOnly()
            : (IReadOnlyList<IEvent>)new List<IEvent>());
    }

    public Task<int> GetCurrentVersionAsync(Guid aggregateId)
    {
        return Task.FromResult(_eventStreams.TryGetValue(aggregateId, out var events)
            ? events.Count
            : 0);
    }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException() : base("Concurrency conflict detected") { }
}
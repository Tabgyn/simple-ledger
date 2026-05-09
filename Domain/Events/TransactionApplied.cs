using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Domain.Events;

public class TransactionApplied : IEvent
{
    public Guid AggregateId { get; }
    public int Version { get; }
    public DateTime Timestamp { get; }
    public TransactionId TransactionId { get; }
    public Name TransactionName { get; }
    public IReadOnlyList<EntryData> Entries { get; }

    public TransactionApplied(Guid aggregateId, int version, TransactionId transactionId, Name transactionName, IReadOnlyList<EntryData> entries)
    {
        AggregateId = aggregateId;
        Version = version;
        Timestamp = DateTime.UtcNow;
        TransactionId = transactionId;
        TransactionName = transactionName;
        Entries = entries;
    }
}

public record EntryData(string Direction, Guid AccountId, decimal Amount);
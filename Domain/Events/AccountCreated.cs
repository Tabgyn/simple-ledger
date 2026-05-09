using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Domain.Events;

public class AccountCreated : IEvent
{
    public Guid AggregateId { get; }
    public int Version { get; }
    public DateTime Timestamp { get; }
    public Name Name { get; }
    public string AccountType { get; } // "debit" or "credit"

    public AccountCreated(Guid aggregateId, int version, Name name, string accountType)
    {
        AggregateId = aggregateId;
        Version = version;
        Timestamp = DateTime.UtcNow;
        Name = name;
        AccountType = accountType;
    }
}
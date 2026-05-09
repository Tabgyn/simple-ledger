using System.Collections.Generic;
using System.Linq;
using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Domain;

public class Transaction
{
    public TransactionId Id { get; }
    public Name Name { get; }
    public IReadOnlyList<Entry> Entries { get; }

    public Transaction(TransactionId id, Name name, IEnumerable<Entry> entries)
    {
        Id = id;
        Name = name;
        Entries = entries.ToList();
        if (!Entries.Any()) throw new InvalidOperationException("Transaction must include at least one entry");
        if (!IsBalanced()) throw new InvalidOperationException("Transaction must balance");
    }

    private bool IsBalanced()
    {
        var total = Entries.Sum(e => e.ContributionToBalance().Amount);
        return total == 0;
    }
}
using System.Collections.Concurrent;
using SimpleLedger.Application.ReadModels;
using SimpleLedger.Domain.Events;

namespace SimpleLedger.Application.Projectors;

public class TransactionProjector
{
    private readonly ConcurrentDictionary<Guid, TransactionReadModel> _transactions = new();

    public void Project(IEnumerable<IEvent> events)
    {
        foreach (var @event in events.OrderBy(e => e.Version))
        {
            if (@event is TransactionApplied applied)
            {
                var transactionId = applied.TransactionId.Value;
                if (!_transactions.ContainsKey(transactionId))
                {
                    var readModel = new TransactionReadModel
                    {
                        Id = transactionId,
                        Name = applied.TransactionName.Value,
                        Entries = applied.Entries.Select(e => new EntryReadModel
                        {
                            Id = Guid.NewGuid(), // We don't have entry IDs in events, generate new ones
                            Direction = e.Direction,
                            Amount = e.Amount,
                            AccountId = e.AccountId
                        }).ToList()
                    };
                    _transactions[transactionId] = readModel;
                }
            }
        }
    }

    public TransactionReadModel? GetTransaction(Guid id)
    {
        return _transactions.TryGetValue(id, out var transaction) ? transaction : null;
    }

    public IEnumerable<TransactionReadModel> GetAllTransactions()
    {
        return _transactions.Values;
    }
}
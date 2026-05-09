using System.Collections.Concurrent;
using System.Threading.Tasks;
using SimpleLedger.Domain;
using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Infrastructure;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly ConcurrentDictionary<TransactionId, Transaction> _transactions = new();

    public Task<Transaction?> Get(TransactionId id) => Task.FromResult(_transactions.GetValueOrDefault(id));

    public Task Save(Transaction transaction)
    {
        _transactions[transaction.Id] = transaction;
        return Task.CompletedTask;
    }
}
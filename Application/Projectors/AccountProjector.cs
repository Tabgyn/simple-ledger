using System.Collections.Concurrent;
using SimpleLedger.Application.ReadModels;
using SimpleLedger.Domain.Events;

namespace SimpleLedger.Application.Projectors;

public class AccountProjector
{
    private readonly ConcurrentDictionary<Guid, AccountReadModel> _accounts = new();

    public void Project(IEnumerable<IEvent> events)
    {
        foreach (var @event in events.OrderBy(e => e.Version))
        {
            if (@event is AccountCreated created)
            {
                var readModel = new AccountReadModel
                {
                    Id = created.AggregateId,
                    Name = created.Name.Value,
                    Direction = created.AccountType,
                    Balance = 0
                };
                _accounts[created.AggregateId] = readModel;
            }
            else if (@event is TransactionApplied applied)
            {
                if (_accounts.TryGetValue(applied.AggregateId, out var account))
                {
                    foreach (var entry in applied.Entries.Where(e => e.AccountId == applied.AggregateId))
                    {
                        if (account.Direction == "debit")
                        {
                            account.Balance += entry.Direction == "debit" ? entry.Amount : -entry.Amount;
                        }
                        else // credit account
                        {
                            account.Balance += entry.Direction == "credit" ? entry.Amount : -entry.Amount;
                        }
                    }
                }
            }
        }
    }

    public AccountReadModel? GetAccount(Guid id)
    {
        return _accounts.TryGetValue(id, out var account) ? account : null;
    }

    public IEnumerable<AccountReadModel> GetAllAccounts()
    {
        return _accounts.Values;
    }
}
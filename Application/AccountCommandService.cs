using System.Collections.Concurrent;
using SimpleLedger.Domain;
using SimpleLedger.Domain.Events;
using SimpleLedger.Domain.ValueObjects;
using SimpleLedger.Infrastructure;

namespace SimpleLedger.Application;

public class AccountCommandService
{
    private readonly IEventStore _eventStore;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _accountLocks = new();

    public AccountCommandService(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Account> CreateAccountAsync(AccountId id, Name name, string accountType)
    {
        var accountId = id.Value;
        var semaphore = _accountLocks.GetOrAdd(accountId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            var currentVersion = await _eventStore.GetCurrentVersionAsync(accountId);
            if (currentVersion > 0)
                throw new InvalidOperationException("Account already exists");

            var @event = new AccountCreated(accountId, 1, name, accountType);
            await _eventStore.AppendEventsAsync(accountId, new[] { @event }, 0);

            return Account.RebuildFromEvents(await _eventStore.GetEventsAsync(accountId));
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<Account> ApplyTransactionAsync(Transaction transaction)
    {
        // For simplicity, we'll apply the transaction to all affected accounts atomically
        // In a real system, this might be more complex with distributed transactions

        var affectedAccountIds = transaction.Entries.Select(e => e.AccountId.Value).Distinct().ToList();
        var semaphores = affectedAccountIds.Select(id => _accountLocks.GetOrAdd(id, _ => new SemaphoreSlim(1, 1))).ToList();

        // Acquire locks in order to prevent deadlocks
        foreach (var semaphore in semaphores.OrderBy(s => s.GetHashCode()))
        {
            await semaphore.WaitAsync();
        }

        try
        {
            // Check that all accounts exist
            foreach (var accountId in affectedAccountIds)
            {
                var version = await _eventStore.GetCurrentVersionAsync(accountId);
                if (version == 0)
                    throw new InvalidOperationException($"Account {accountId} not found");
            }

            // Apply transaction to all accounts
            var events = new List<IEvent>();
            var entryData = transaction.Entries.Select(e => new EntryData(
                e is DebitEntry ? "debit" : "credit",
                e.AccountId.Value,
                e.Amount.Amount)).ToList();

            foreach (var accountId in affectedAccountIds)
            {
                var currentVersion = await _eventStore.GetCurrentVersionAsync(accountId);
                var @event = new TransactionApplied(accountId, currentVersion + 1, transaction.Id, transaction.Name, entryData);
                events.Add(@event);
            }

            // Append events with optimistic concurrency
            foreach (var accountId in affectedAccountIds)
            {
                var accountEvents = events.Where(e => e.AggregateId == accountId).ToList();
                var expectedVersion = await _eventStore.GetCurrentVersionAsync(accountId);
                await _eventStore.AppendEventsAsync(accountId, accountEvents, expectedVersion);
            }

            // Return one of the updated accounts (for simplicity, the first one)
            var firstAccountId = affectedAccountIds.First();
            return Account.RebuildFromEvents(await _eventStore.GetEventsAsync(firstAccountId));
        }
        catch (ConcurrencyException)
        {
            // Retry logic could be added here
            throw new InvalidOperationException("Concurrent modification detected, please retry");
        }
        finally
        {
            foreach (var semaphore in semaphores)
            {
                semaphore.Release();
            }
        }
    }

    public async Task<Account?> GetAccountAsync(AccountId id)
    {
        var events = await _eventStore.GetEventsAsync(id.Value);
        if (!events.Any()) return null;
        return Account.RebuildFromEvents(events);
    }
}
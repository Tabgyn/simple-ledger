using SimpleLedger.Domain;
using SimpleLedger.Domain.Events;
using SimpleLedger.Domain.ValueObjects;
using SimpleLedger.Infrastructure;

namespace SimpleLedger.Application;

public class LedgerService
{
    private readonly AccountCommandService _accountCommandService;
    private readonly IEventStore _eventStore;
    private readonly IdempotencyService _idempotencyService;

    public LedgerService(AccountCommandService accountCommandService, IEventStore eventStore, IdempotencyService idempotencyService)
    {
        _accountCommandService = accountCommandService;
        _eventStore = eventStore;
        _idempotencyService = idempotencyService;
    }

    public async Task<Account> CreateAccountAsync(string idempotencyKey, AccountId id, Name name, string accountType)
    {
        if (_idempotencyService.IsProcessed(idempotencyKey))
        {
            throw new IdempotencyException("Request already processed");
        }

        var account = await _accountCommandService.CreateAccountAsync(id, name, accountType);
        _idempotencyService.MarkAsProcessed(idempotencyKey, account);
        return account;
    }

    public async Task ApplyTransactionAsync(string idempotencyKey, Transaction transaction)
    {
        if (_idempotencyService.IsProcessed(idempotencyKey))
        {
            return; // Idempotent - do nothing
        }

        await _accountCommandService.ApplyTransactionAsync(transaction);
        _idempotencyService.MarkAsProcessed(idempotencyKey, true);
    }

    public async Task<Account?> GetAccountAsync(AccountId id)
    {
        return await _accountCommandService.GetAccountAsync(id);
    }
}

public class IdempotencyException : Exception
{
    public IdempotencyException(string message) : base(message) { }
}
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SimpleLedger.Domain;
using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Infrastructure;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly ConcurrentDictionary<AccountId, Account> _accounts = new();

    public Task<Account?> Get(AccountId id) => Task.FromResult(_accounts.GetValueOrDefault(id));

    public Task Save(Account account)
    {
        _accounts[account.Id] = account;
        return Task.CompletedTask;
    }
}
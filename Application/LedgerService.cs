using System.Threading.Tasks;
using SimpleLedger.Domain;

namespace SimpleLedger.Application;

public class LedgerService
{
    private readonly IAccountRepository _accountRepository;

    public LedgerService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task ApplyTransaction(Transaction transaction)
    {
        foreach (var entry in transaction.Entries)
        {
            var account = await _accountRepository.Get(entry.AccountId);
            if (account == null) throw new InvalidOperationException("Account not found");
            entry.ApplyTo(account);
            await _accountRepository.Save(account);
        }
    }
}
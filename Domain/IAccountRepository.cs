using System.Threading.Tasks;
using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Domain;

public interface IAccountRepository
{
    Task<Account?> Get(AccountId id);
    Task Save(Account account);
}
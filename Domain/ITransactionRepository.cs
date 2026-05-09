using System.Threading.Tasks;
using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Domain;

public interface ITransactionRepository
{
    Task<Transaction?> Get(TransactionId id);
    Task Save(Transaction transaction);
}
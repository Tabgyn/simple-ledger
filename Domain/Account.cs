using SimpleLedger.Domain.Events;
using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Domain;

public abstract class Account
{
    public AccountId Id { get; }
    public Name Name { get; }
    public Money Balance { get; protected set; }
    public int Version { get; protected set; }

    protected Account(AccountId id, Name name)
    {
        Id = id;
        Name = name;
        Balance = Money.Zero;
        Version = 0;
    }

    protected Account(AccountId id, Name name, Money balance, int version)
    {
        Id = id;
        Name = name;
        Balance = balance;
        Version = version;
    }

    public abstract Account ApplyEvent(IEvent @event);

    public static Account RebuildFromEvents(IEnumerable<IEvent> events)
    {
        Account? account = null;
        foreach (var @event in events.OrderBy(e => e.Version))
        {
            if (@event is AccountCreated created)
            {
                account = created.AccountType == "debit"
                    ? new DebitAccount(new AccountId(created.AggregateId), created.Name, Money.Zero, created.Version)
                    : new CreditAccount(new AccountId(created.AggregateId), created.Name, Money.Zero, created.Version);
            }
            else if (account != null)
            {
                account = account.ApplyEvent(@event);
            }
        }
        return account ?? throw new InvalidOperationException("No AccountCreated event found");
    }
}

public class DebitAccount : Account
{
    public DebitAccount(AccountId id, Name name) : base(id, name) { }

    internal DebitAccount(AccountId id, Name name, Money balance, int version) : base(id, name, balance, version) { }

    public override Account ApplyEvent(IEvent @event)
    {
        if (@event is TransactionApplied transactionApplied)
        {
            var newBalance = Balance;
            foreach (var entry in transactionApplied.Entries.Where(e => e.AccountId == Id.Value))
            {
                var amount = Money.FromDecimal(entry.Amount);
                newBalance = entry.Direction == "debit"
                    ? newBalance.Add(amount)
                    : newBalance.Subtract(amount);
            }
            return new DebitAccount(Id, Name, newBalance, @event.Version);
        }
        return this;
    }
}

public class CreditAccount : Account
{
    public CreditAccount(AccountId id, Name name) : base(id, name) { }

    internal CreditAccount(AccountId id, Name name, Money balance, int version) : base(id, name, balance, version) { }

    public override Account ApplyEvent(IEvent @event)
    {
        if (@event is TransactionApplied transactionApplied)
        {
            var newBalance = Balance;
            foreach (var entry in transactionApplied.Entries.Where(e => e.AccountId == Id.Value))
            {
                var amount = Money.FromDecimal(entry.Amount);
                newBalance = entry.Direction == "credit"
                    ? newBalance.Add(amount)
                    : newBalance.Subtract(amount);
            }
            return new CreditAccount(Id, Name, newBalance, @event.Version);
        }
        return this;
    }
}
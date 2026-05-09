using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Domain;

public abstract class Account
{
    public AccountId Id { get; }
    public Name Name { get; }
    public Money Balance { get; protected set; }

    protected Account(AccountId id, Name name)
    {
        Id = id;
        Name = name;
        Balance = Money.Zero;
    }

    public abstract void ApplyDebitEntry(Money amount);
    public abstract void ApplyCreditEntry(Money amount);
}

public class DebitAccount : Account
{
    public DebitAccount(AccountId id, Name name) : base(id, name) { }

    public override void ApplyDebitEntry(Money amount) => Balance = Balance.Add(amount);
    public override void ApplyCreditEntry(Money amount) => Balance = Balance.Subtract(amount);
}

public class CreditAccount : Account
{
    public CreditAccount(AccountId id, Name name) : base(id, name) { }

    public override void ApplyDebitEntry(Money amount) => Balance = Balance.Subtract(amount);
    public override void ApplyCreditEntry(Money amount) => Balance = Balance.Add(amount);
}
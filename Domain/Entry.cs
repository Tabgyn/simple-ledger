using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Domain;

public abstract class Entry
{
    public EntryId Id { get; }
    public Money Amount { get; }
    public AccountId AccountId { get; }

    protected Entry(EntryId id, Money amount, AccountId accountId)
    {
        Id = id;
        Amount = amount;
        AccountId = accountId;
    }

    public abstract void ApplyTo(Account account);
    public abstract Money ContributionToBalance();
}

public class DebitEntry : Entry
{
    public DebitEntry(EntryId id, Money amount, AccountId accountId) : base(id, amount, accountId) { }

    public override void ApplyTo(Account account) => account.ApplyDebitEntry(Amount);
    public override Money ContributionToBalance() => Amount;
}

public class CreditEntry : Entry
{
    public CreditEntry(EntryId id, Money amount, AccountId accountId) : base(id, amount, accountId) { }

    public override void ApplyTo(Account account) => account.ApplyCreditEntry(Amount);
    public override Money ContributionToBalance() => Amount.Negate();
}
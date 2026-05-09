using SimpleLedger.Domain;
using SimpleLedger.Domain.Events;
using SimpleLedger.Domain.ValueObjects;

namespace tests.Domain;

[TestClass]
public class AccountTests
{
    [TestMethod]
    public void DebitAccount_applies_debit_entries_as_increase()
    {
        var accountId = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new AccountCreated(accountId, 1, new Name("Assets"), "debit"),
            new TransactionApplied(accountId, 2, new TransactionId(Guid.NewGuid()), new Name("Test"), new List<EntryData> { new("debit", accountId, 100m) })
        };

        var account = Account.RebuildFromEvents(events);

        Assert.AreEqual(100m, account.Balance.Amount);
        Assert.AreEqual("Assets", account.Name.Value);
    }

    [TestMethod]
    public void DebitAccount_applies_credit_entries_as_decrease()
    {
        var accountId = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new AccountCreated(accountId, 1, new Name("Assets"), "debit"),
            new TransactionApplied(accountId, 2, new TransactionId(Guid.NewGuid()), new Name("Test"), new List<EntryData> { new("debit", accountId, 100m) }),
            new TransactionApplied(accountId, 3, new TransactionId(Guid.NewGuid()), new Name("Test"), new List<EntryData> { new("credit", accountId, 40m) })
        };

        var account = Account.RebuildFromEvents(events);

        Assert.AreEqual(60m, account.Balance.Amount);
    }

    [TestMethod]
    public void CreditAccount_applies_credit_entries_as_increase()
    {
        var accountId = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new AccountCreated(accountId, 1, new Name("Liability"), "credit"),
            new TransactionApplied(accountId, 2, new TransactionId(Guid.NewGuid()), new Name("Test"), new List<EntryData> { new("credit", accountId, 100m) })
        };

        var account = Account.RebuildFromEvents(events);

        Assert.AreEqual(100m, account.Balance.Amount);
    }

    [TestMethod]
    public void CreditAccount_applies_debit_entries_as_decrease()
    {
        var accountId = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new AccountCreated(accountId, 1, new Name("Liability"), "credit"),
            new TransactionApplied(accountId, 2, new TransactionId(Guid.NewGuid()), new Name("Test"), new List<EntryData> { new("credit", accountId, 100m) }),
            new TransactionApplied(accountId, 3, new TransactionId(Guid.NewGuid()), new Name("Test"), new List<EntryData> { new("debit", accountId, 40m) })
        };

        var account = Account.RebuildFromEvents(events);

        Assert.AreEqual(60m, account.Balance.Amount);
    }
}

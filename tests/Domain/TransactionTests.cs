using System;
using System.Collections.Generic;
using SimpleLedger.Domain;
using SimpleLedger.Domain.ValueObjects;

namespace tests.Domain;

[TestClass]
public class TransactionTests
{
    [TestMethod]
    public void Balanced_transaction_constructs_successfully()
    {
        var entries = new List<Entry>
        {
            new DebitEntry(new EntryId(Guid.NewGuid()), Money.FromDecimal(100m), new AccountId(Guid.NewGuid())),
            new CreditEntry(new EntryId(Guid.NewGuid()), Money.FromDecimal(100m), new AccountId(Guid.NewGuid()))
        };

        var transaction = new Transaction(new TransactionId(Guid.NewGuid()), new Name("Transfer"), entries);

        Assert.AreEqual(2, transaction.Entries.Count);
    }

    [TestMethod]
    public void Unbalanced_transaction_throws()
    {
        var entries = new List<Entry>
        {
            new DebitEntry(new EntryId(Guid.NewGuid()), Money.FromDecimal(100m), new AccountId(Guid.NewGuid())),
            new CreditEntry(new EntryId(Guid.NewGuid()), Money.FromDecimal(90m), new AccountId(Guid.NewGuid()))
        };

        Assert.ThrowsException<InvalidOperationException>(() => new Transaction(new TransactionId(Guid.NewGuid()), new Name("Bad"), entries));
    }

    [TestMethod]
    public void Empty_transaction_throws()
    {
        var entries = new List<Entry>();

        Assert.ThrowsException<InvalidOperationException>(() => new Transaction(new TransactionId(Guid.NewGuid()), new Name("Empty"), entries));
    }
}

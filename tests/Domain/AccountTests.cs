using SimpleLedger.Domain;
using SimpleLedger.Domain.ValueObjects;

namespace tests.Domain;

[TestClass]
public class AccountTests
{
    [TestMethod]
    public void DebitAccount_applies_debit_entries_as_increase()
    {
        var account = new DebitAccount(new AccountId(Guid.NewGuid()), new Name("Assets"));
        account.ApplyDebitEntry(Money.FromDecimal(100m));

        Assert.AreEqual(100m, account.Balance.Amount);
    }

    [TestMethod]
    public void DebitAccount_applies_credit_entries_as_decrease()
    {
        var account = new DebitAccount(new AccountId(Guid.NewGuid()), new Name("Assets"));
        account.ApplyDebitEntry(Money.FromDecimal(100m));
        account.ApplyCreditEntry(Money.FromDecimal(40m));

        Assert.AreEqual(60m, account.Balance.Amount);
    }

    [TestMethod]
    public void CreditAccount_applies_credit_entries_as_increase()
    {
        var account = new CreditAccount(new AccountId(Guid.NewGuid()), new Name("Liability"));
        account.ApplyCreditEntry(Money.FromDecimal(100m));

        Assert.AreEqual(100m, account.Balance.Amount);
    }

    [TestMethod]
    public void CreditAccount_applies_debit_entries_as_decrease()
    {
        var account = new CreditAccount(new AccountId(Guid.NewGuid()), new Name("Liability"));
        account.ApplyCreditEntry(Money.FromDecimal(100m));
        account.ApplyDebitEntry(Money.FromDecimal(40m));

        Assert.AreEqual(60m, account.Balance.Amount);
    }
}

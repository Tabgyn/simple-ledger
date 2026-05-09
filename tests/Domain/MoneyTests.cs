using System;
using SimpleLedger.Domain.ValueObjects;

namespace tests.Domain;

[TestClass]
public class MoneyTests
{
    [TestMethod]
    public void FromDecimal_positive_amount_creates_money()
    {
        var money = Money.FromDecimal(100m);

        Assert.AreEqual(100m, money.Amount);
        Assert.AreEqual("USD", money.Currency);
    }

    [TestMethod]
    public void FromDecimal_zero_amount_throws()
    {
        Assert.ThrowsException<InvalidOperationException>(() => Money.FromDecimal(0m));
    }

    [TestMethod]
    public void FromDecimal_negative_amount_throws()
    {
        Assert.ThrowsException<InvalidOperationException>(() => Money.FromDecimal(-1m));
    }
}

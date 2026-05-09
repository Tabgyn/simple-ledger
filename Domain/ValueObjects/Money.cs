using System;

namespace SimpleLedger.Domain.ValueObjects;

public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency ?? "USD";
    }

    public static Money FromDecimal(decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("Money amount must be positive");
        return new(amount, "USD");
    }

    public static Money Zero => new(0, "USD");

    public Money Add(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException("Currencies must match");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException("Currencies must match");
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Negate() => new(-Amount, Currency);

    public override bool Equals(object? obj) => obj is Money m && Amount == m.Amount && Currency == m.Currency;
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount} {Currency}";
}
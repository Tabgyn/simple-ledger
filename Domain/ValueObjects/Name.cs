namespace SimpleLedger.Domain.ValueObjects;

public sealed record Name
{
    public string Value { get; }

    public Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Name must be provided", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
namespace SimpleLedger.Application.ReadModels;

public class TransactionReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<EntryReadModel> Entries { get; set; } = new();
}

public class EntryReadModel
{
    public Guid Id { get; set; }
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid AccountId { get; set; }
}
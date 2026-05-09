using SimpleLedger.Domain.ValueObjects;

namespace SimpleLedger.Application.ReadModels;

public class AccountReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
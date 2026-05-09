using System.Text.Json.Serialization;
using SimpleLedger.Application;
using SimpleLedger.Application.Projectors;
using SimpleLedger.Application.ReadModels;
using SimpleLedger.Domain;
using SimpleLedger.Domain.ValueObjects;
using SimpleLedger.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
builder.Services.AddSingleton<AccountCommandService>();
builder.Services.AddSingleton<IdempotencyService>();
builder.Services.AddSingleton<AccountProjector>();
builder.Services.AddSingleton<TransactionProjector>();
builder.Services.AddSingleton<LedgerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/accounts", async (CreateAccountRequest request, LedgerService ledgerService, AccountProjector projector, HttpContext context) =>
{
    var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
    if (string.IsNullOrEmpty(idempotencyKey))
        return Results.BadRequest(new { error = "Idempotency-Key header is required" });

    if (!TryValidateCreateAccountRequest(request, out var problem))
        return problem!;

    var accountId = request.Id ?? Guid.NewGuid();
    var name = new Name(string.IsNullOrWhiteSpace(request.Name) ? "Unnamed" : request.Name.Trim());
    var direction = request.Direction.Trim().ToLowerInvariant();

    try
    {
        var account = await ledgerService.CreateAccountAsync(idempotencyKey, new AccountId(accountId), name, direction);

        // Project the events to update read models
        var events = await app.Services.GetRequiredService<IEventStore>().GetEventsAsync(accountId);
        projector.Project(events);

        var readModel = projector.GetAccount(accountId);
        if (readModel == null) return Results.Problem("Failed to create account read model");

        return Results.Ok(new AccountResponse(readModel.Id, readModel.Name, readModel.Direction, readModel.Balance));
    }
    catch (IdempotencyException)
    {
        // For idempotency, return the existing account
        var readModel = projector.GetAccount(accountId);
        if (readModel == null) return Results.NotFound();
        return Results.Ok(new AccountResponse(readModel.Id, readModel.Name, readModel.Direction, readModel.Balance));
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapGet("/accounts/{id:guid}", async (Guid id, AccountProjector projector) =>
{
    var account = projector.GetAccount(id);
    if (account is null)
        return Results.NotFound();

    return Results.Ok(new AccountResponse(account.Id, account.Name, account.Direction, account.Balance));
});

app.MapPost("/transactions", async (CreateTransactionRequest request, LedgerService ledgerService, AccountProjector accountProjector, TransactionProjector transactionProjector, IEventStore eventStore, HttpContext context) =>
{
    var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
    if (string.IsNullOrEmpty(idempotencyKey))
        return Results.BadRequest(new { error = "Idempotency-Key header is required" });

    if (!TryValidateCreateTransactionRequest(request, out var problem))
        return problem!;

    var entries = new List<Entry>();
    foreach (var entryRequest in request.Entries)
    {
        var amount = Money.FromDecimal(entryRequest.Amount);
        var accountId = new AccountId(entryRequest.AccountId);
        var entry = entryRequest.Direction.Trim().ToLowerInvariant() switch
        {
            "debit" => new DebitEntry(new EntryId(Guid.NewGuid()), amount, accountId) as Entry,
            "credit" => new CreditEntry(new EntryId(Guid.NewGuid()), amount, accountId),
            _ => throw new InvalidOperationException("Unreachable entry direction validation")
        };

        entries.Add(entry);
    }

    Transaction transaction;
    try
    {
        transaction = new Transaction(request.Id.HasValue ? new TransactionId(request.Id.Value) : new TransactionId(Guid.NewGuid()), new Name(string.IsNullOrWhiteSpace(request.Name) ? "Unnamed" : request.Name.Trim()), entries);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Transaction must balance", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new { error = ex.Message });
    }

    try
    {
        await ledgerService.ApplyTransactionAsync(idempotencyKey, transaction);

        // Project events to update read models
        var affectedAccountIds = transaction.Entries.Select(e => e.AccountId.Value).Distinct();
        foreach (var accountId in affectedAccountIds)
        {
            var events = await eventStore.GetEventsAsync(accountId);
            accountProjector.Project(events);
        }

        // Project transaction events (assuming transaction events are on accounts)
        var transactionEvents = await eventStore.GetEventsAsync(affectedAccountIds.First());
        transactionProjector.Project(transactionEvents.Where(e => e is SimpleLedger.Domain.Events.TransactionApplied));

        var transactionReadModel = transactionProjector.GetTransaction(transaction.Id.Value);
        if (transactionReadModel == null) return Results.Problem("Failed to create transaction read model");

        return Results.Ok(new TransactionResponse(transactionReadModel.Id, transactionReadModel.Name, transactionReadModel.Entries.Select(e => new EntryResponse(e.Id, e.Direction, e.Amount, e.AccountId)).ToList()));
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Account not found", StringComparison.OrdinalIgnoreCase))
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrent modification", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

static bool TryValidateCreateAccountRequest(CreateAccountRequest request, out IResult? problem)
{
    var errors = new Dictionary<string, string[]>();

    if (request.Id.HasValue && request.Id.Value == Guid.Empty)
        errors["id"] = new[] { "Id cannot be empty." };

    if (string.IsNullOrWhiteSpace(request.Direction))
        errors["direction"] = new[] { "Direction is required." };
    else if (request.Direction.Trim().ToLowerInvariant() != "debit" && request.Direction.Trim().ToLowerInvariant() != "credit")
        errors["direction"] = new[] { "Direction must be 'debit' or 'credit'." };

    if (errors.Count > 0)
    {
        problem = Results.ValidationProblem(errors);
        return false;
    }

    problem = null;
    return true;
}

static bool TryValidateCreateTransactionRequest(CreateTransactionRequest request, out IResult? problem)
{
    var errors = new Dictionary<string, string[]>();

    if (request.Id.HasValue && request.Id.Value == Guid.Empty)
        errors["id"] = new[] { "Id cannot be empty." };

    if (request.Entries is null || request.Entries.Count == 0)
        errors["entries"] = new[] { "Transaction requires at least one entry." };
    else
    {
        for (var i = 0; i < request.Entries.Count; i++)
        {
            var entry = request.Entries[i];
            var index = i.ToString();
            if (string.IsNullOrWhiteSpace(entry.Direction))
                errors[$"entries[{index}].direction"] = new[] { "Entry direction is required." };
            else if (entry.Direction.Trim().ToLowerInvariant() != "debit" && entry.Direction.Trim().ToLowerInvariant() != "credit")
                errors[$"entries[{index}].direction"] = new[] { "Entry direction must be 'debit' or 'credit'." };

            if (entry.AccountId == Guid.Empty)
                errors[$"entries[{index}].account_id"] = new[] { "Entry account_id cannot be empty." };

            if (entry.Amount <= 0)
                errors[$"entries[{index}].amount"] = new[] { "Entry amount must be greater than zero." };
        }
    }

    if (errors.Count > 0)
    {
        problem = Results.ValidationProblem(errors);
        return false;
    }

    problem = null;
    return true;
}

app.Run();

public sealed record CreateAccountRequest([property: JsonPropertyName("id")] Guid? Id, [property: JsonPropertyName("name")] string? Name, [property: JsonPropertyName("direction")] string Direction);
public sealed record AccountResponse(Guid Id, string Name, string Direction, decimal Balance);
public sealed record CreateEntryRequest([property: JsonPropertyName("direction")] string Direction, [property: JsonPropertyName("account_id")] Guid AccountId, [property: JsonPropertyName("amount")] decimal Amount);
public sealed record CreateTransactionRequest([property: JsonPropertyName("id")] Guid? Id, [property: JsonPropertyName("name")] string? Name, [property: JsonPropertyName("entries")] List<CreateEntryRequest> Entries);
public sealed record TransactionResponse(Guid Id, string Name, List<EntryResponse> Entries);
public sealed record EntryResponse(Guid Id, string Direction, decimal Amount, Guid AccountId);

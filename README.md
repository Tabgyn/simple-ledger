# Simple Ledger

A double-entry accounting ledger implemented in C# and .NET using Domain-Driven Design principles with Event Sourcing, CQRS, and ACID guarantees.

## Features

- **Event Sourcing**: Immutable audit trail of all changes
- **CQRS**: Separate read and write models for optimal performance
- **ACID Simulation**: Concurrency control with optimistic locking
- **Idempotency**: Duplicate request prevention via idempotency keys
- **Auditability**: Complete history of all state changes
- Create accounts (debit or credit)
- Create balanced transactions that update account balances
- In-memory storage for simplicity
- RESTful API with command/query separation

## Architecture

This project implements several advanced patterns:

### Event Sourcing
- All state changes are captured as immutable events
- Aggregates are rebuilt from event streams
- Full audit trail maintained automatically

### CQRS (Command Query Responsibility Segregation)
- Commands (writes) use event-sourced aggregates
- Queries (reads) use optimized read models built from event projections
- Clear separation between write and read concerns

### ACID Simulation
- Account-level concurrency control using semaphores
- Optimistic concurrency with version checking
- Atomic transaction application across multiple accounts

### Idempotency
- All write operations require `Idempotency-Key` header
- Prevents duplicate processing of requests
- Client-provided UUIDs recommended

## Setup

1. Ensure .NET 10.0 or later is installed.
2. Clone or download the project.
3. Run `dotnet build` to build the project.
4. Run `dotnet run` to start the server on https://localhost:5000.

## Dependencies

- .NET 10.0
- ASP.NET Core

## API Endpoints

The API follows CQRS principles with separate endpoints for commands (writes) and queries (reads). All write operations require an `Idempotency-Key` header.

### Commands (Writes)

#### POST /accounts

Create a new account.

**Headers:**
```
Idempotency-Key: <uuid>
```

**Request body:**
```json
{
  "name": "Discretionary Funds",
  "direction": "debit",
  "id": "71cde2aa-b9bc-496a-a6f1-34964d05e6fd"
}
```

**Response:**
```json
{
  "id": "71cde2aa-b9bc-496a-a6f1-34964d05e6fd",
  "name": "Discretionary Funds",
  "direction": "debit",
  "balance": 0.0
}
```

#### POST /transactions

Create a new transaction.

**Headers:**
```
Idempotency-Key: <uuid>
```

**Request body:**
```json
{
  "name": "Withdrawal",
  "id": "3256dc3c-7b18-4a21-95c6-146747cf2971",
  "entries": [
    {
      "direction": "debit",
      "account_id": "fa967ec9-5be2-4c26-a874-7eeeabfc6da8",
      "amount": 100
    },
    {
      "direction": "credit",
      "account_id": "dbf17d00-8701-4c4e-9fc5-6ae33c324309",
      "amount": 100
    }
  ]
}
```

**Response:**
```json
{
  "id": "3256dc3c-7b18-4a21-95c6-146747cf2971",
  "name": "Withdrawal",
  "entries": [
    {
      "id": "generated-id",
      "direction": "debit",
      "amount": 100.0,
      "accountId": "fa967ec9-5be2-4c26-a874-7eeeabfc6da8"
    },
    {
      "id": "generated-id",
      "direction": "credit",
      "amount": 100.0,
      "accountId": "dbf17d00-8701-4c4e-9fc5-6ae33c324309"
    }
  ]
}
```

### Queries (Reads)

#### GET /accounts/{id}

Get account details from read model.

**Response:**
```json
{
  "id": "71cde2aa-b9bc-496a-a6f1-34964d05e6fd",
  "name": "Discretionary Funds",
  "direction": "debit",
  "balance": 150.0
}
```

## Error Handling

The API returns appropriate HTTP status codes:

- `200 OK`: Success
- `400 Bad Request`: Validation errors (invalid data)
- `404 Not Found`: Account not found
- `409 Conflict`: Concurrency conflict (retry with new idempotency key)
- `422 Unprocessable Entity`: Business rule violations (unbalanced transactions)

## Idempotency

All write operations are idempotent. If you send the same request with the same `Idempotency-Key`, it will return the same result without processing again. Use unique UUIDs for each distinct operation.

## Concurrency

The system handles concurrent operations safely:
- Account-level locking prevents race conditions
- Optimistic concurrency detects conflicts
- Failed operations should be retried with new idempotency keys

## Running Tests

The solution includes a test project at `tests/tests.csproj`.

Run the full test suite with:

```bash
dotnet test tests/tests.csproj
```

All currently implemented tests pass for the API, domain model, and new architectural patterns.
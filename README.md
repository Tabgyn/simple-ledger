# Simple Ledger

A double-entry accounting ledger implemented in C# and .NET using Domain-Driven Design principles.

## Features

- Create accounts (debit or credit)
- Create balanced transactions that update account balances
- In-memory storage for simplicity
- RESTful API

## Setup

1. Ensure .NET 10.0 or later is installed.
2. Clone or download the project.
3. Run `dotnet build` to build the project.
4. Run `dotnet run` to start the server on https://localhost:5000.

## Dependencies

- .NET 10.0
- ASP.NET Core

## API Endpoints

Minimal API endpoints are exposed directly from `Program.cs`, using record DTOs for request and response shapes.

### POST /accounts

Create a new account.

Request body:
```json
{
  "name": "Discretionary Funds",
  "direction": "debit",
  "id": "71cde2aa-b9bc-496a-a6f1-34964d05e6fd"
}
```

### GET /accounts/{id}

Get account details.

### POST /transactions

Create a new transaction.

Request body:
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

## Running Tests

The solution includes a test project at `tests/tests.csproj`.

Run the full test suite with:

```bash
dotnet test tests/tests.csproj
```

All currently implemented tests pass for the API and domain model.
# Plan: Designing a Double-Entry Ledger with DDD Principles

Build a maintainable, extensible domain model for a double-entry accounting ledger using C# and .NET, adhering to strict DDD rules: no primitive obsession, no enums for state, no nullable business fields, no invalid method exposure, no anemic models, no large conditional logic trees.

## Steps

- Identify domain concepts: Account, Transaction, Entry, Money, Direction, Id, Name.
- Define invariants: Transactions must balance (sum of debit amounts = sum of credit amounts); Account balances updated only via transactions; Directions affect balance calculations correctly.
- Identify invalid states: Unbalanced transactions; Entries with invalid directions; Accounts with null required fields; Direct balance modifications.
- Design types to eliminate invalid states: Use value objects for Money, Direction (as sealed class hierarchy), Id (Guid wrapper), Name (string wrapper); Polymorphic account and entry types (DebitAccount/CreditAccount, DebitEntry/CreditEntry) to avoid conditionals; Transaction aggregate enforces balance in constructor; Accounts as aggregates with behavior to apply entries.
- Define behavior per type: Accounts have ApplyEntry method (polymorphic); Transactions created with balanced entries; Entries contribute to balance checks via polymorphism.
- Implement in-memory repositories for aggregates.
- Implement API controllers using domain services.
- Write unit tests for domain logic.
- Create README.md with setup and run instructions.

## Relevant files

- Domain/Account.cs — DebitAccount, CreditAccount classes
- Domain/Transaction.cs — Transaction aggregate
- Domain/Entry.cs — DebitEntry, CreditEntry classes
- Domain/ValueObjects/Money.cs
- Domain/ValueObjects/Direction.cs
- Domain/ValueObjects/AccountId.cs
- Domain/ValueObjects/TransactionId.cs
- Domain/ValueObjects/EntryId.cs
- Domain/ValueObjects/Name.cs
- Infrastructure/InMemoryAccountRepository.cs
- Infrastructure/InMemoryTransactionRepository.cs
- Program.cs — ASP.NET setup & Minimal APIs
- README.md

## Verification

- Unit tests: Test transaction balance invariant; account balance updates for debit/credit entries; polymorphic behavior.
- Integration tests: API endpoints return correct responses; in-memory storage persists data.
- Manual: Run API calls from examples; verify balances update correctly.

## Decisions

- Use polymorphism for direction handling to eliminate conditionals: DebitAccount.ApplyDebitEntry adds to balance, etc.
- Make name required in domain to avoid nullable fields; API can default if needed.
- Initial account balance always 0; balances only modified via transactions.
- Ids generated if not provided in API.
- In-memory storage for simplicity, no database.

## Further Considerations

Extensibility: How to add new account types (e.g., expense accounts) without changing core logic?
Currency: Currently USD only; how to extend to multiple currencies?
Validation: Use domain events or exceptions for invalid operations?
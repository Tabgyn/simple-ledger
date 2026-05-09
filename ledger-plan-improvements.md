## Plan: Guaranteeing Immutability, ACID, Idempotency, CQRS, and Auditability in Simple-Ledger

Implement the five guarantees while maintaining the DDD architecture and in-memory storage. Use event sourcing for immutability and auditability, simulate ACID with locks and versioning, add CQRS with in-memory projections, and implement idempotency via request keys.

**Steps**

1. **Introduce Event Sourcing Infrastructure**
   - Create `IEvent` interface and concrete event classes (e.g., `AccountCreated`, `TransactionApplied`).
   - Implement `IEventStore` interface with in-memory storage using `ConcurrentDictionary`.
   - Add event versioning and optimistic concurrency checks.
   - *Parallel with step 2*

2. **Refactor Aggregates to Event-Sourced (Immutability & Auditability)**
   - Modify `Account` and `Transaction` to be immutable: remove mutating methods, add `ApplyEvent` to rebuild from events.
   - Implement aggregate reconstruction from event streams.
   - Ensure full auditability: all changes logged as events.
   - *Depends on step 1*

3. **Implement ACID Simulation**
   - Add locks (e.g., `SemaphoreSlim`) for account-level concurrency.
   - Implement serializable isolation: version checks on aggregates, retry logic on conflicts.
   - Wrap transaction application in atomic blocks with rollback on failure.
   - *Depends on step 2*

4. **Implement CQRS with In-Memory Projections**
   - Create read models (e.g., `AccountReadModel`, `TransactionReadModel`) separate from write aggregates.
   - Implement projectors that build read models from events.
   - Update API to use read models for queries, write aggregates for commands.
   - *Depends on step 1*

5. **Add Mandatory Idempotency**
   - Store processed request keys in `ConcurrentDictionary<string, bool>`.
   - Require `Idempotency-Key` header in API requests.
   - Check and set key before processing; return cached response if duplicate.
   - *Parallel with other steps*

6. **Update Application Services**
   - Modify `LedgerService` to use event store and projections.
   - Ensure all operations are idempotent and atomic.
   - *Depends on steps 2,3,4,5*

7. **Update API Endpoints**
   - Add idempotency key validation.
   - Separate command and query endpoints explicitly.
   - Return responses from read models for queries.
   - *Depends on step 6*

**Relevant files**
- `Domain/Events/IEvent.cs` — Event interface
- `Domain/Events/AccountCreated.cs`, `TransactionApplied.cs`, etc. — Event classes
- `Infrastructure/EventStore.cs` — In-memory event store with concurrency
- `Domain/Account.cs` — Refactored to event-sourced aggregate
- `Domain/Transaction.cs` — Refactored similarly
- `Application/ReadModels/AccountReadModel.cs` — CQRS read model
- `Application/Projectors/AccountProjector.cs` — Builds read models
- `Application/IdempotencyService.cs` — Handles idempotency keys
- `Program.cs` — Updated API with CQRS separation and idempotency

**Verification**
1. **Unit Tests**: Test event sourcing rebuilds aggregates correctly; ACID simulation prevents concurrent conflicts; idempotency blocks duplicates.
2. **Integration Tests**: API rejects duplicate requests; concurrent transactions serialize correctly; read models reflect latest events.
3. **Manual Tests**: Run multiple concurrent transactions; verify balances and audit logs; test idempotency with same key.
4. **Load Test**: Simulate high concurrency to verify ACID simulation holds.

**Decisions**
- **In-Memory Persistence**: Keep for simplicity, simulate ACID with locks/versioning instead of real DB.
- **Custom Event Sourcing**: Implement without external libraries for learning/control.
- **Idempotency Keys**: Client-provided via header; stored in memory; suggest UUIDs.
- **CQRS Read Models**: In-memory projections updated synchronously for simplicity.
- **Auditability**: Event stream provides full immutable history.
- **Scope**: Focus on core guarantees; exclude multi-currency, reversals for now.

**Further Considerations**
1. **Performance**: In-memory may not scale; consider real DB later.
2. **Error Handling**: Add comprehensive exception handling for conflicts/retries.
3. **Monitoring**: Add logging for events and conflicts.
4. **Testing**: Increase test coverage for concurrency scenarios.
# Authority.Records — Architecture & Component Reference

## Overview

Authority.Records is a law-enforcement records management system built with **.NET 9**, **Blazor Server**, and **SQL Server**. It follows a **Domain-Driven Design (DDD)** architecture with clean-layer separation, **CQRS** (Command/Query Responsibility Segregation) via MediatR, and an **Outbox pattern** for reliable domain event delivery.

The system manages three core record types — **Incidents**, **Arrests**, and **Citations** — all of which support a multi-jurisdiction tenancy model, a lifecycle state machine, pessimistic record locking, and soft deletes.

---

## Solution Structure

```
Authority.Records.sln
├── Modules.Records.Domain           # Core domain model (entities, events, policies)
├── Modules.Records.Application      # Use cases (commands, queries, handlers, DTOs)
├── Shared.Infrastructure            # Cross-cutting concerns (EF Core, outbox, identity)
├── Modules.Records.UI               # Blazor Server front-end
├── Api                              # (Minimal API host — currently unused by UI)
├── Modules.Records.Domain.Tests     # Domain unit tests
├── Modules.Records.Application.Tests# Application layer unit tests
└── Infrastructure.IntegrationTests  # Outbox + EF Core integration tests
```

---

## Project: `Modules.Records.Domain`

The innermost layer. Has **zero dependencies** on frameworks or infrastructure. Contains everything that belongs to the domain model.

### Aggregate Roots

All aggregate roots inherit from a layered primitive hierarchy:

```
Entity
 └─ AggregateRoot                  (domain events, soft delete, RowVersion)
     └─ StatefulAggregateRoot<T>   (RecordStatus state machine, ChangeStatus)
         └─ LockableAggregateRoot<T> (pessimistic locking: AcquireLock / ReleaseLock)
```

#### `Incident`
- Core case aggregate for incident-specific workflow and lifecycle.
- Lifecycle: `Draft → Open → Closed → Archived`
- Participates in relationships with `Arrest` and `Citation` through explicit association records rather than in-memory child collections on the aggregate itself.
- Cross-aggregate rules such as incident closure across linked arrests/citations are enforced through repositories, invariants, and domain/application services rather than `Incident` directly owning child entities.
- Overrides `AcquireLock` for future extensibility.
- Private EF Core constructor `private Incident() {}` prevents materialization from raising domain events.

#### `Arrest`
- Independent lockable aggregate that can be associated to one or more incidents through `IncidentArrestLink`.
- Tracks: `SuspectName`, `ArrestedAt`, `IsFinalized`.
- Lifecycle: `Draft → Open → Closed → Archived`
- May also carry an optional `PrimaryIncidentId` reference for workflow convenience, but the main relationship model is still association-based.
- Has a finalization step (`IsFinalized = true`) required before the incident can close (if jurisdiction rules require it).
- Private EF Core constructor `private Arrest() {}` — **critical**: prevents spurious `ArrestCreatedDomainEvent` when EF Core materializes the entity from the database.

#### `Citation`
- Independent aggregate that can be associated to incidents through `IncidentCitationLink`.
- Tracks: `Description`, `IssueDate`, `IsIssued`.
- Lifecycle: `Draft → Open → Closed → Archived`
- Uses the same `LockableAggregateRoot<T>` locking and lifecycle primitives as `Incident` and `Arrest`.
- Private EF Core constructor `private Citation() {}`.

#### `Charge`
- Reference-data aggregate used by incidents, arrests, and citations through explicit link records.
- Tracks charge catalog fields such as `OffenseName`, `UcrCode`, `ChargeLevel`, `IsCitationEligible`, and activation state.
- Charges are intentionally queried directly from the write model today; there is no dedicated `ChargeReadModel`.
- This is a deliberate trade-off because charges currently behave more like shared catalog/master data than workflow-heavy record aggregates.

### Association Aggregates

- `IncidentArrestLink` models the association between an `Incident` and an `Arrest`.
- `IncidentCitationLink` models the association between an `Incident` and a `Citation`.
- These links represent cross-aggregate relationships, not child ownership inside the `Incident` aggregate boundary.

### Primitive Base Classes (`Common/Primitives/`)

| Class | Responsibility |
|---|---|
| `Entity` | `Id` (Guid), equality by identity |
| `AggregateRoot` | Domain event list, `ClearDomainEvents`, `Version`, `IsDeleted`, `RowVersion` (optimistic concurrency), soft-delete/restore hooks |
| `StatefulAggregateRoot<T>` | `Status` (RecordStatus enum), `ChangeStatus` which validates via `ILifecyclePolicy` and raises `LifecycleStatusChangedDomainEvent` |
| `LockableAggregateRoot<T>` | `LockedByUserId`, `LockedAtUtc`, `IsLocked`, `AcquireLock` / `ReleaseLock` raising `LockAcquiredDomainEvent` / `LockReleasedDomainEvent`. Uses injected `IAuthorizationPolicy`, `ILockExpirationStrategy`, `IClock` |

### Domain Events (`DomainEvents/`)

Events are plain records that implement `IDomainEvent`. Base class `DomainEvent` carries `EventId`, `OccurredOnUtc`, `AggregateId`, and `AggregateVersion` (set automatically by `AggregateRoot.AddDomainEvent`).

**Incident events:** `IncidentCreatedDomainEvent`, `IncidentSoftDeletedDomainEvent`, `IncidentRestoredDomainEvent`

**Arrest events:** `ArrestCreatedDomainEvent`, `ArrestSoftDeletedDomainEvent`, `ArrestRestoredDomainEvent`

**Citation events:** `CitationCreatedDomainEvent`, `CitationIssuedDomainEvent`, `CitationSoftDeletedDomainEvent`, `CitationRestoredDomainEvent`

**Charge events:** `ChargeCreatedDomainEvent`, `ChargeUpdatedDomainEvent`, `ChargeActivatedDomainEvent`, `ChargeDeactivatedDomainEvent`, `ChargeDeletedDomainEvent`

**Generic events (any aggregate):**
- `LifecycleStatusChangedDomainEvent<T>` — raised on any status transition
- `LockAcquiredDomainEvent<T>` — raised when a lock is acquired
- `LockReleasedDomainEvent<T>` — raised when a lock is released

### Policies (`Common/Policies/`)

Policies are pure domain objects injected into aggregates (no infrastructure dependencies).

| Interface | Responsibility |
|---|---|
| `ILifecyclePolicy<T>` | `ValidateTransition` — enforces valid state transitions and delegates to close policy when needed |
| `IClosePolicy<T>` | Validates whether an aggregate can move to `Closed` status |
| `IAuthorizationPolicy<T>` | `EnsureCanAcquireLock`, `EnsureCanReleaseLock`, `EnsureCanModify` |
| `ILockExpirationStrategy<T>` | `IsLockActive` — determines if a lock is still valid given a timeout |

Concrete close policies:
- `IncidentClosePolicy` — delegates to `IJurisdictionRulesService` to check if arrests/citations must be closed/finalized first
- `ArrestClosePolicy` — checks `IsFinalized` if required by jurisdiction rules
- `CitationClosePolicy` — checks `IsIssued` if required by jurisdiction rules
- `CompositeClosePolicy<T>` — chains multiple policies with AND semantics

### Specifications (`Common/Specifications/`)

A composable Specification pattern for domain invariant expressions.

```csharp
var spec = new AllArrestsFinalizedSpecification()
    .And(new AllCitationsIssuedSpecification());
```

Implementations: `AllArrestsClosedSpecification`, `AllArrestsFinalizedSpecification`, `AllCitationsIssuedSpecification`, `ArrestDateNotFutureSpecification`, `SuspectNameProvidedSpecification`, `CitationIssuedSpecification`, `IssueDateNotFutureSpecification`.

### Domain Invariants (`DomainInvariants/`)

Invariants wrap specifications and provide structured `DomainInvariantResult` objects with error codes. `CompositeDomainInvariant` chains multiple invariants. Used by `IncidentCloseDomainService`.

### Domain Services (`Services/`)

`IncidentCloseDomainService` — coordinates incident closure across linked aggregates: loads related arrests/citations through repositories, runs invariants, delegates to `ILifecyclePolicy`, and handles force-close logic.

### Factories (`Factories/`)

`IncidentFactory` and `ArrestFactory` wrap aggregate constructors. The `internal` constructor access ensures aggregates can only be created through factories or from within the domain assembly.

### Value Objects (`ValueObjects/`)

`RecordNumber` — typed wrapper around a formatted record number string.  
`Address` — street/city/state/zip with equality by value.

---

## Project: `Modules.Records.Application`

The use-case layer. Depends only on `Modules.Records.Domain`. Implements CQRS via **MediatR**.

### Commands and Queries

Each feature folder (Arrests, Citations, Incidents) contains:

```
Commands/
  CreateArrest/
    CreateArrestCommand.cs       — IRequest<Guid>
    CreateArrestHandler.cs       — IRequestHandler<CreateArrestCommand, Guid>
    CreateArrestValidator.cs     — AbstractValidator<CreateArrestCommand>
Queries/
  GetArrestById/
    GetArrestByIdQuery.cs        — IRequest<ArrestDto?>
    GetArrestByIdHandler.cs
```

**Arrest commands:** `CreateArrest`, `OpenArrest`, `CloseArrest`, `FinalizeArrest`, `ArchiveArrest`, `SoftDeleteArrest`, `RestoreArrest`, `AcquireArrestLock`, `ReleaseArrestLock`

**Incident commands:** `CreateIncident`, `OpenIncident`, `CloseIncident`, `ArchiveIncident`, `UpdateIncidentDescription`, `SoftDeleteIncident`, `RestoreIncident`, `AcquireIncidentLock`, `ReleaseIncidentLock`

**Citation commands:** `CreateCitation`, `IssueCitation`, `SoftDeleteCitation`, `RestoreCitation`, `AcquireCitationLock`, `ReleaseCitationLock`

### Projection Handlers (Domain Event → Read Model)

`ArrestProjectionHandler`, `IncidentProjectionHandler`, `CitationProjectionHandler` implement `INotificationHandler<T>` for their respective domain events and maintain the read model tables.

Each handler is **idempotent** — it checks for prior existence before inserting (to safely handle the double-dispatch pattern from the outbox + synchronous dispatch).

`ArrestProjectionHandler` also handles:
- `LockAcquiredDomainEvent<Arrest>` → sets `IsLocked = true`, `LockedByUserId`
- `LockReleasedDomainEvent<Arrest>` → clears lock fields

Charges are the intentional exception: application queries read directly from `Charges` rather than through a projected `ChargeReadModel`. Revisit that decision only if denormalized usage metrics, external synchronization hooks, or charge-query performance become a real need.

### Read Models (`ReadModels/`)

Denormalized projections optimized for UI queries. They are **not** aggregate roots — they have no domain events and do not participate in the domain model.

| Model | Notable fields |
|---|---|
| `IncidentReadModel` | Id, JurisdictionId, AgencyId, Description, Status, ArrestCount, CitationCount, IsLocked, LockedByUserId |
| `ArrestReadModel` | Id, IncidentId, SuspectName, ArrestedAt, Status, IsLocked, LockedByUserId |
| `CitationReadModel` | Id, IncidentId, Description, IssueDate, IsIssued, IsLocked, LockedByUserId |

### DTOs (`DTOs/`)

`IncidentDto`, `ArrestDto`, `CitationDto` — returned from query handlers to the UI layer. Mapped directly from read models.

### Pipeline Behaviors (`Common/Behaviors/`)

`ValidationBehavior<TRequest, TResponse>` — MediatR open pipeline behavior. Collects all registered `IValidator<TRequest>` implementations and throws `ValidationException` if any fail. Registered globally via `cfg.AddOpenBehavior(typeof(ValidationBehavior<,>))`.

### Assembly Registration (`AssemblyReference.cs`)

`AddApplication()` extension method registers:
- MediatR with handler scanning from this assembly
- `ValidationBehavior` as an open pipeline behavior
- FluentValidation validators from this assembly

---

## Project: `Shared.Infrastructure`

Cross-cutting infrastructure. Depends on both domain and application layers.

### Persistence (`Persistence/`)

#### `AppDbContext`

Central EF Core `DbContext` for all domain entities and read models. Key design decisions:

- **Registered as `Transient`** — each MediatR handler receives its own `DbContext` instance, preventing Blazor Server SignalR concurrency conflicts between simultaneous user actions.
- **`SaveChangesAsync` override** implements the outbox pattern:
  1. `UpdateRowVersions()` — stamps `RowVersion` on all modified entities with that property
  2. Collects `DomainEvents` from all tracked `AggregateRoot` entities in `Added/Modified/Deleted` state
  3. Writes each event as an `OutboxMessage` row
  4. Calls `base.SaveChangesAsync` — commits entity changes and outbox messages atomically
  5. Clears domain events from entities
  6. Calls `_domainEventDispatcher.DispatchAsync` — synchronous in-process dispatch for immediate projection consistency

- **Global query filters** applied automatically to all `AggregateRoot`-derived entities:
  - Soft delete: `WHERE IsDeleted = false`
  - Multi-tenancy: `WHERE JurisdictionId = @CurrentTenantId`

#### `AuthDbContext`

Separate EF Core context for ASP.NET Core Identity tables (`ApplicationUser`, roles, claims). Registered as the default Scoped lifetime.

#### EF Core Configurations (`Persistence/Configurations/`)

Fluent API configurations for all entities. Notable:
- `IncidentConfiguration` — maps `_arrests` and `_citations` private backing fields for child collections
- `OutboxMessageConfiguration` — maps `RowVersion` as a concurrency token
- `ArrestReadModelConfiguration` / `IncidentReadModelConfiguration` / `CitationReadModelConfiguration` — map read models to their dedicated tables

### Outbox Pattern (`Outbox/`)

Provides **at-least-once delivery** of domain events across process restarts.

#### `OutboxMessage`
Stored record of a pending domain event. Fields: `Id`, `JurisdictionId`, `AggregateId`, `AggregateVersion`, `OccurredOnUtc`, `Type` (assembly-qualified name), `Content` (JSON), `ProcessingStartedOnUtc`, `ProcessedOnUtc`, `RetryCount`, `NextRetryOnUtc` (exponential backoff), `IsFailedPermanently`, `RowVersion`.

#### `OutboxProcessor` (Background Service)
Runs every 5 seconds. Per iteration:
1. Queries unprocessed messages ordered by `OccurredOnUtc`
2. **Claims** each message atomically by setting `ProcessingStartedOnUtc` and saving with optimistic concurrency (`RowVersion`) — concurrent processors will get a `DbUpdateConcurrencyException` and skip the message
3. Deserializes the event using `DomainEventTypeRegistry`
4. Dispatches via `IDomainEventDispatcher`
5. Marks message as processed or failed (with exponential backoff up to `MaxRetries`)
6. Permanently failed messages are moved to `DeadLetterMessages`

#### `DomainEventTypeRegistry`
Singleton. Scans the domain assembly at startup for all `IDomainEvent` implementations and builds a dictionary keyed by assembly-qualified type name. Used by `OutboxProcessor` to deserialize messages.

#### `DeadLetterQueueWriter` / `DeadLetterMessage`
Permanently failed outbox messages are moved to `DeadLetterMessages` for inspection and manual replay.

#### `OutboxCleanupProcessor` / `OutboxCleanupService`
Background service that periodically deletes processed outbox messages older than a configurable retention period (`OutboxCleanupOptions`).

### Audit Trail (`Audit/`)

#### `AuditTrailDomainEventHandler`
Registered as `INotificationHandler<IDomainEvent>`. Intended to record all domain events into `AuditTrailEntries`. ⚠️ **Known limitation:** MediatR dispatches notifications using the runtime concrete type; this handler is registered for the `IDomainEvent` interface and is not currently invoked for concrete event types (e.g., `ArrestCreatedDomainEvent`).

#### `AuditInterceptor`
EF Core `SaveChangesInterceptor`. Captures `Added/Modified/Deleted` entity changes and writes `AuditTrailEntry` records directly to the database — separate from the domain-event-based audit trail.

#### `AuditTrailEntry`
Stores: `EventId`, `EventType`, `OccurredOnUtc`, `JurisdictionId`, `AggregateId`, `AggregateVersion`, `Payload` (JSON).

### Identity (`Identity/`)

#### `ApplicationUser`
Extends `IdentityUser` with `JurisdictionId` (Guid) and `AgencyId` (Guid) — the two custom claims written into the auth cookie.

#### `RecordsUserClaimsPrincipalFactory`
Overrides `UserClaimsPrincipalFactory<ApplicationUser>` to add `jurisdiction` and `agency` claims to the identity cookie when a user signs in.

#### `HttpTenantProvider`
Implements `ITenantProvider` by reading `jurisdiction`, `agency`, and `NameIdentifier` claims from `IHttpContextAccessor.HttpContext.User`. Also supports `SetJurisdictionId` for use by the `OutboxProcessor` when processing background messages (no HTTP context available).

### Domain Event Dispatching (`DomainEvents/`)

`MediatRDomainEventDispatcher` wraps `IMediator.Publish`. Called by `AppDbContext.SaveChangesAsync` after a successful save to dispatch events synchronously, and by `OutboxProcessor` for background delivery.

### Jurisdiction Services (`Jurisdiction/`)

`JurisdictionRulesService` — loads `JurisdictionConfiguration` from the database and provides typed rule checks:
- `MustCloseAllArrests` — whether all arrests must be closed before an incident can close
- `MustCloseAllCitations` — whether all citations must be closed before an incident can close

`JurisdictionConfigurationRepository` — EF Core repository for `JurisdictionConfiguration` entities.

### Dependency Injection (`DependencyInjection.cs`)

`AddInfrastructure(IConfiguration)` registers all infrastructure services. Notable registrations:

| Service | Lifetime | Notes |
|---|---|---|
| `AppDbContext` | Transient | Includes `AuditInterceptor` |
| `AuthDbContext` | Scoped | ASP.NET Identity |
| `IApplicationDbContext` | Transient | Delegate to `AppDbContext` |
| `ITenantProvider` | Scoped | `HttpTenantProvider` (overridden to `BlazorTenantProvider` in UI) |
| `IDomainEventDispatcher` | Scoped | `MediatRDomainEventDispatcher` |
| `OutboxProcessor` | Hosted Service | Background loop |
| `ILifecyclePolicy<T>` | Scoped | One per aggregate type, wraps `IClosePolicy<T>` |
| `IClosePolicy<T>` | Scoped | `CompositeClosePolicy` wrapping aggregate-specific policies |
| `IModificationContext` | Scoped | `UserModificationContext` (userId from `ITenantProvider`) |
| `DomainEventTypeRegistry` | Singleton | Scans domain assembly once |

---

## Project: `Modules.Records.UI`

Blazor Server front-end. Uses `InteractiveServer` render mode globally (`App.razor`).

### Service Layer (`Services/`)

Thin wrappers around MediatR `ISender`. Blazor components never reference MediatR or command/query types directly — they call through service interfaces.

| Service | Key operations |
|---|---|
| `IncidentService` | GetByJurisdiction, GetById, Create, Open, Close, Archive, UpdateDescription, AcquireLock, ReleaseLock, SoftDelete, Restore |
| `ArrestService` | GetByIncident, GetById, Create, Open, Close, Finalize, Archive, AcquireLock, ReleaseLock, SoftDelete, Restore |
| `CitationService` | GetByIncident, GetById, Create, Issue, AcquireLock, ReleaseLock, SoftDelete, Restore |

### Tenant Provider (`BlazorTenantProvider`)

Overrides `HttpTenantProvider` for Blazor Server circuits. During SignalR interactions, `IHttpContextAccessor.HttpContext` is `null`. `BlazorTenantProvider` falls back to `AuthenticationStateProvider` to read claims when the HTTP context is unavailable.

### Pages

#### `IncidentList.razor`
Displays all incidents for the current user's jurisdiction. Reads from `IncidentReadModel` via `GetIncidentsByJurisdictionQuery`.

#### `IncidentDetails.razor`
Shows incident fields, status badge, lock badge, and linked arrest/citation lists. Supports:
- **Modify mode** — clicking "Modify" calls `AcquireIncidentLock`, starts a countdown timer (`LockTimeout = 10 min`), and enables editing actions
- **Release** — calls `ReleaseIncidentLock`, stops the timer, exits modify mode
- **Lock timer** — a `System.Threading.Timer` ticks every second; when it reaches zero it auto-releases the lock and navigates appropriately
- `DisposeAsync` stops the timer only — does **not** release the lock (intentional: navigating away does not release the lock)

#### `IncidentCreate.razor`
Simple form. Uses a `_submitting` guard to prevent double-submit. On success, navigates to the new incident's detail page.

#### `ArrestDetails.razor` / `CitationDetails.razor`
Same modify-mode pattern as `IncidentDetails`. Includes lifecycle action buttons (Open, Finalize, Close, Archive) visible only in modify mode.

#### `ArrestCreate.razor` / `CitationCreate.razor`
Forms for creating independent aggregates and associating them to incidents from incident-centric workflows. They currently require the parent incident workflow to be in modify mode before linking from that page flow.

### Components (`Components/`)

| Component | Responsibility |
|---|---|
| `StatusBadge` | Renders a Bootstrap badge colored by `RecordStatus` value |
| `LockBadge` | Shows 🔒 with locked-by user info, or nothing when unlocked |
| `ConfirmDialog` | Modal confirmation dialog with `OnConfirm` / `OnCancel` callbacks |
| `NavMenu` | Application navigation bar with links to Incidents, and a Sign Out button |
| `RedirectToLogin` | Renders nothing; navigates to `/account/login` when the user is not authenticated |

### Authentication Pages

Razor Pages (not Blazor components) because they require full HTTP request/response lifecycle:

- `Login.cshtml` — Email/password form, calls `SignInManager.PasswordSignInAsync`, sets the cookie, and redirects
- `Logout.cshtml` — Calls `SignOutAsync` and redirects to `/`

### Authorization (`Authorization/`)

`RecordsAuthorizationPolicies.RegisterPolicies` configures `IAuthorizationService` policies. Currently enforces `RequireAuthenticatedUser`.

---

## Cross-Cutting Patterns

### Multi-Tenancy

Every `AggregateRoot` that implements `IMultiTenant` has a `JurisdictionId` property. EF Core global query filters automatically scope all queries to `WHERE JurisdictionId = @CurrentTenantId`. The current tenant is resolved from the `jurisdiction` JWT/cookie claim by `BlazorTenantProvider`.

### Pessimistic Record Locking

Before modifying any record, the user must enter **Modify Mode** by acquiring a lock:
1. UI calls `AcquireLock` command → handler calls `aggregate.AcquireLock(context, timeout)` → saves → dispatches `LockAcquiredDomainEvent`
2. Projection handler updates `ArrestReadModel.IsLocked = true` and `LockedByUserId`
3. UI starts a countdown timer. When it expires or when the user clicks Release, `ReleaseLock` is called
4. A second user attempting to acquire the lock while it is active gets a `DomainException`
5. Users with `CanOverrideLocks = true` in their `IModificationContext` can break any lock

### Outbox + Synchronous Dual Dispatch

`AppDbContext.SaveChangesAsync` **both** persists events to the outbox (durable) and dispatches them synchronously in-process (fast). This means:
- The UI sees projections updated immediately after a command
- If the process crashes before the outbox is processed, events will still be delivered on restart
- Projection handlers must be **idempotent** to safely handle both dispatches

### Soft Delete

All aggregates support `SoftDelete(userId)` / `Restore(userId)`. The EF Core global filter `WHERE IsDeleted = false` hides soft-deleted records from all queries automatically.

### Optimistic Concurrency (`RowVersion`)

`AggregateRoot.RowVersion` is a `byte[]` updated to `Guid.NewGuid().ToByteArray()` on every `SaveChangesAsync` call (via `UpdateRowVersions`). EF Core uses this as a concurrency token. `OutboxMessage` also carries a `RowVersion` token used by `OutboxProcessor` to claim messages without duplicate processing.

---

## Data Flow: Creating an Arrest

```
User clicks "Add Arrest"
  └─ ArrestCreate.razor.SubmitAsync
      └─ ArrestService.CreateAsync(...)
          └─ ISender.Send(CreateArrestCommand)
              └─ [ValidationBehavior] validates command
              └─ CreateArrestHandler.Handle
                  ├─ ArrestFactory.Create(...) — new Arrest(), raises ArrestCreatedDomainEvent
                  ├─ _dbContext.Arrests.Add(arrest)
                  ├─ _dbContext.SaveChangesAsync() — persists Arrest + outbox message
                  ├─ Resolve requested incident associations
                  ├─ Create `IncidentArrestLink` records
                  └─ _dbContext.SaveChangesAsync()
                      ├─ Writes OutboxMessage(ArrestCreatedDomainEvent / link events) to DB
                      ├─ base.SaveChangesAsync() — commits changes atomically per save call
                      └─ _domainEventDispatcher.DispatchAsync(...)
                          └─ Projection handlers update arrest and incident read models
  └─ Nav.NavigateTo("/incidents/{id}") — shows updated incident with new arrest
  [5 seconds later] OutboxProcessor picks up OutboxMessage
      └─ ArrestProjectionHandler.Handle(ArrestCreatedDomainEvent) — idempotency check exits early
```

---

## Known Issues / Limitations

| Issue | Description |
|---|---|
| `AuditTrailDomainEventHandler` never fires | Registered as `INotificationHandler<IDomainEvent>` but MediatR dispatches using the concrete runtime type, so this handler receives no events. The `AuditTrailEntries` table remains empty. |
| `Citation` locking | `Citation` implements its own lock fields rather than inheriting from `LockableAggregateRoot<T>`, resulting in inconsistent lock behavior compared to `Incident` and `Arrest`. |
| No REST API | The `Api` project exists but is not wired to a running host. All operations go through the Blazor Server UI directly via MediatR in-process. |
| Dev seed user only | Only one hard-coded dev user (`admin@authority.local`) exists. No user management UI is implemented. |

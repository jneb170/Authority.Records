# Records Module Blueprint

This document is the blueprint for adding future records modules such as `Accidents`, `Locations`, `Vehicles`, or `Warrants`.

It captures the common structure used by the current records modules (`Incidents`, `Arrests`, and `Citations`) so new modules fit the existing architecture instead of introducing a parallel pattern.

## What a records module usually includes

Most records modules span all of these layers:

1. Domain entity or aggregate in `Modules.Records.Domain\Entities`
2. Domain events, factory, and any module-specific policies/invariants
3. Application commands, queries, handlers, and validators in `Modules.Records.Application\<Module>`
4. Read model and projection handlers in `Modules.Records.Application\ReadModels` and `...\DomainEventHandlers`
5. EF Core configuration and `DbSet` exposure through `IApplicationDbContext` / `AppDbContext`
6. Blazor service wrapper in `Modules.Records.UI\Services`
7. Blazor pages/components in `Modules.Records.UI\Pages\<Module>`
8. Unit and integration tests

If a new module skips one of these, do it intentionally and document why.

## Common behaviors existing records modules share

Current records modules are not just CRUD tables. They usually participate in a common records lifecycle.

- Multi-tenancy via `JurisdictionId` and `AgencyId`
- Soft delete and restore
- Domain events on create, update, delete, restore, lock, and status changes
- Read-model projection for UI queries
- Outbox persistence plus synchronous in-process dispatch
- Record-number generation using `AgencySequenceCounter` and agency format configuration
- Audit metadata (`CreatedBy`, `ModifiedBy`, timestamps) in read models and persisted events
- Concurrency handling through `RowVersion` where the entity participates in optimistic concurrency

When creating a new module, start from the assumption that it should follow these behaviors unless there is a clear reason not to.

## Recommended module shape

### 1. Domain layer

Create the primary entity in `Modules.Records.Domain\Entities`.

For top-level records, prefer the same model used by `Incident`:

- implement `IMultiTenant`
- inherit from `LockableAggregateRoot<T>` when the record should support record locking and lifecycle transitions
- use a private parameterless constructor for EF materialization
- keep creation logic in the domain constructor or a domain factory
- raise domain events inside domain methods instead of in handlers

For child or linked records, decide whether the module is:

- a top-level aggregate like `Incident`
- a child record that can still be operated on directly like `Arrest`
- a supporting/link entity like `IncidentArrestLink`

If the module needs locking, use the shared locking primitives instead of inventing a separate lock model. `Citation` currently diverges from this and should be treated as an exception, not the template.

Add the domain pieces that match the module:

- `<Module>.cs`
- `<Module>CreatedDomainEvent`
- `<Module>DetailsUpdatedDomainEvent`
- `<Module>SoftDeletedDomainEvent`
- `<Module>RestoredDomainEvent`
- status and lock events only if the module participates in those behaviors
- `<Module>Factory` when creation should be centralized
- close policy / lifecycle policy hooks if the module has state transitions

### 2. Application layer

Create a module folder in `Modules.Records.Application\<Module>`.

Follow the existing feature-folder pattern:

```text
<Module>\
  Commands\
    Create<Module>\
    Update<Module>Details\
    SoftDelete<Module>\
    Restore<Module>\
    Acquire<Module>Lock\
    Release<Module>Lock\
    Open<Module>\
    Close<Module>\
    Archive<Module>\
  Queries\
    Get<Module>ById\
    Get<Module>ByRecordNumber\
    Get<Module>sByJurisdiction\
```

Use the same conventions as the current modules:

- commands and queries are `sealed record` types implementing `IRequest<T>`
- handlers are `sealed` classes
- validators live beside the request and use FluentValidation
- handlers work through `IApplicationDbContext`
- UI-facing queries should generally read from read models, not aggregate tables

If the module needs related-record linking, add explicit commands and queries for the relationship, like the existing arrest/citation incident link flows.

## Read models and projections

Each current records module has a dedicated read model used for UI queries.

Create a read model in `Modules.Records.Application\ReadModels` when the module has its own list/detail screens or needs denormalized query fields.

A typical read model includes:

- `Id`
- `RecordNumber`
- `JurisdictionId`
- `AgencyId`
- flattened display/query fields
- `Status` when lifecycle is used
- `IsDeleted`
- `IsLocked` and `LockedByUserId` when locking is used
- `CreatedBy`, `ModifiedBy`, `CreatedAtUtc`, `UpdatedAtUtc`

Also add:

- a `Create(...)` factory method
- mutation methods like `ApplyDetailsChanged`, `ApplyStatusChange`, `ApplyDeleted`, `ApplyRestored`, `ApplyLockAcquired`, `ApplyLockReleased`
- `ToDto()` mapping so query handlers stay thin

Projection handlers belong under `Modules.Records.Application\<Module>\DomainEventHandlers`.

Projection handler rules:

- implement `INotificationHandler<TEvent>` for each domain event the read model cares about
- keep handlers idempotent
- save the read model immediately after each change
- update lock/state/delete flags from domain events rather than recomputing ad hoc in UI code

If the module has relationship read models, add dedicated projection handlers for them too.

## Infrastructure work required

Every new records module usually needs infrastructure wiring in `Shared.Infrastructure`.

At minimum, check these areas:

- `Shared.Infrastructure\Persistence\AppDbContext.cs`
- `Modules.Records.Application\Abstractions\IApplicationDbContext.cs`
- `Shared.Infrastructure\Persistence\Configurations\`
- `Shared.Infrastructure\DependencyInjection.cs`

Typical work items:

- add `DbSet<<Module>>` to `IApplicationDbContext`
- add `DbSet<<Module>>` and any supporting/link `DbSet`s to `AppDbContext`
- add the read model `DbSet` if applicable
- add EF configuration for the aggregate and read model
- configure indexes, max lengths, uniqueness rules, and concurrency tokens
- ignore computed value-object properties when EF should not map them directly
- register factories, repositories, and policies if the module uses them

Because `AppDbContext.SaveChangesAsync()` automatically writes domain events to the outbox and dispatches them in-process, new module events join that pipeline automatically once the entity is tracked and emits events correctly.

## Number generation pattern

If the new module has an agency-formatted record number, follow the pattern used in `CreateIncidentHandler`, `CreateArrestHandler`, and `CreateCitationHandler`.

That means:

- store the DB identity as `RecordNumber`
- store the agency-visible formatted string separately (`IncidentNum`, `ArrestNum`, `CitationNum`, etc.)
- read the module format from `AgencyConfigurations`
- reserve sequence numbers via `AgencySequenceCounter`
- retry on concurrency conflicts
- fall back to a system default format when agency configuration is missing

When possible, extract shared logic instead of cloning another `TryGenerate...NumAsync` method for every new module.

## UI layer pattern

The UI does not use HTTP APIs for record workflows. It talks to MediatR through scoped services in `Modules.Records.UI\Services`.

For a new module, create:

- `I<Module>Service`
- `<Module>Service`
- pages/components under `Modules.Records.UI\Pages\<Module>`

The service should remain a thin wrapper around commands and queries.

Typical UI flows include:

- list page
- create page
- details page
- edit/modify flow
- lock acquire/release flow when the module is editable and lock-protected

If the module supports creation-time auto-numbering, mirror the existing create-page behavior:

- allow leaving the module number blank
- optionally offer a “Generate” button
- call a configuration service or command for previewing/generating the next number
- navigate to the created record using `RecordNumber`

## Testing expectations

New modules should be added with tests in the same style as the existing codebase.

### Domain tests

Add focused tests for:

- creation
- update behavior
- state transitions
- locking rules
- soft delete/restore
- emitted domain events

### Application tests

Add handler/query tests where the module has non-trivial orchestration, validation, or mapping.

### Integration tests

Add integration coverage when the module depends on:

- EF configuration
- global query filters
- outbox/event projection behavior
- concurrency-sensitive number generation
- relationship/link projection behavior

Use the existing in-memory SQLite integration style rather than inventing a different infrastructure test setup.

## Module build checklist

Use this as the minimum checklist for a new records module.

### Domain

- [ ] Add entity and constructor/factory
- [ ] Add module domain events
- [ ] Add lifecycle/close/authorization policies if needed
- [ ] Add value objects or invariants if the module needs them

### Application

- [ ] Add create/update/delete/restore commands
- [ ] Add lock and status commands when applicable
- [ ] Add list/detail queries
- [ ] Add validators
- [ ] Add DTOs or reuse existing DTO patterns
- [ ] Add projection handlers
- [ ] Add read model and mapping methods

### Infrastructure

- [ ] Add `DbSet` entries to `IApplicationDbContext`
- [ ] Add `DbSet` entries to `AppDbContext`
- [ ] Add EF Core configurations
- [ ] Register factories/policies/services in DI
- [ ] Add migrations if schema changes are ready

### UI

- [ ] Add service interface and implementation
- [ ] Add list/create/details pages
- [ ] Add edit/modify flows
- [ ] Add linking UI if the module participates in cross-record relationships

### Tests

- [ ] Add domain tests
- [ ] Add application tests where useful
- [ ] Add integration tests for persistence, projections, and filters

## Module-specific decisions to make up front

Before implementation, answer these questions for the new module:

1. Is it a top-level aggregate, a child record, or a link/supporting entity?
2. Does it need lifecycle transitions (`Draft`, `Open`, `Closed`, `Archived`)?
3. Does it need pessimistic locking?
4. Does it need an agency-formatted visible record number?
5. Does it need its own read model?
6. Does it link to incidents or other records?
7. Does closing another record depend on it?
8. Does it need jurisdiction-rule-driven close/finalize checks?
9. Which fields should be queryable/sortable in the UI and therefore flattened into the read model?

These decisions drive almost every other part of the implementation.

## Recommended starting point for a new module

When creating a new records module:

1. Start from `Incident` if it is a top-level locked record with lifecycle support.
2. Start from `Arrest` if it is an operational child/direct record with linking behavior.
3. Only use `Citation` as a partial reference for creation/update/read-model shape; do not copy its custom locking approach as the preferred pattern.

Then build the module downward through:

1. domain entity and events
2. factory and policies
3. application commands/queries/validators
4. read model and projection handlers
5. infrastructure `DbSet` and configuration wiring
6. UI service and pages
7. tests

## Reference files

Use these files as the primary references while building a new module:

- `ARCHITECTURE.md`
- `Modules.Records.Domain\Entities\Incident.cs`
- `Modules.Records.Domain\Entities\Arrest.cs`
- `Modules.Records.Application\Incidents\Commands\CreateIncident\CreateIncidentHandler.cs`
- `Modules.Records.Application\Arrests\Commands\CreateArrest\CreateArrestHandler.cs`
- `Modules.Records.Application\Incidents\DomainEventHandlers\IncidentProjectionHandler.cs`
- `Modules.Records.Application\ReadModels\IncidentReadModel.cs`
- `Modules.Records.UI\Services\IncidentService.cs`
- `Modules.Records.UI\Services\ArrestService.cs`
- `Shared.Infrastructure\Persistence\AppDbContext.cs`
- `Shared.Infrastructure\Persistence\Configurations\IncidentConfiguration.cs`
- `Shared.Infrastructure\DependencyInjection.cs`

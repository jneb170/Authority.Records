# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and test commands

Use the solution root as the working directory.

```bash
dotnet restore
dotnet build ./Authority.Records.sln --no-restore -c Release
dotnet test ./Authority.Records.sln --no-build -c Release
```

Run an individual test project for faster feedback:

```bash
dotnet test ./Modules.Records.Domain.Tests/Modules.Records.Domain.Tests.csproj
dotnet test ./Modules.Records.Application.Tests/Modules.Records.Application.Tests.csproj
dotnet test ./Infrastructure.IntegrationTests/Infrastructure.IntegrationTests.csproj
dotnet test ./Api.Tests/Api.Tests.csproj
```

Run a single test using `FullyQualifiedName` filtering (more reliable than category filtering here):

```bash
dotnet test ./Modules.Records.Domain.Tests/Modules.Records.Domain.Tests.csproj --filter "FullyQualifiedName~Modules.Records.Domain.Tests.Entities.NameTests.Constructor_CreatesPerson_WithAllRequiredFields"
```

CI skips integration tests with `--filter "Category!=Integration"`. Check `.github/workflows/deploy.yml` before changing build, migration, publish, or deployment behavior.

## Architecture overview

Authority.Records is a law-enforcement records management system (RMS) for incident, arrest, and citation tracking. It is a layered .NET 9 application. Read `ARCHITECTURE.md` before making large cross-cutting changes.

**Projects:**
- `Modules.Records.UI` — Blazor Server main host; registers `AddApplication()` and `AddInfrastructure(...)`, overrides `ITenantProvider` with the Blazor-aware implementation, uses scoped UI services as thin wrappers over MediatR.
- `Modules.Records.Application` — CQRS use cases organized by aggregate area (`Incidents`, `Arrests`, `Citations`, etc.) with MediatR requests, handlers, and FluentValidation validators.
- `Modules.Records.Domain` — Aggregates, factories, lifecycle/state rules, soft-delete, optimistic concurrency, and domain events.
- `Shared.Infrastructure` — EF Core persistence, Identity, tenant resolution, audit, locking cleanup, outbox processing, and read-model background services.
- `Api` — Lightweight worker host for infrastructure processing (outbox processor); not a primary application boundary.

**Core flow:**

```
UI service → MediatR command/query → domain aggregate(s) via IApplicationDbContext and factories → EF Core persistence → domain event dispatch → read-model/projector updates
```

**Core aggregates:** `Incident` (Draft → Open → Closed → Archived), `Arrest`, `Citation`, `Charge` (reference data). Cross-aggregate relationships are `IncidentArrestLink` and `IncidentCitationLink`.

## Key conventions

- **No HTTP endpoints** for the core records workflow. UI services in `Modules.Records.UI/Services/*.cs` call MediatR directly.
- **`AppDbContext` is transient** — each MediatR handler gets its own EF Core context to avoid Blazor Server circuit concurrency issues. Do not change this to scoped.
- **`AuthDbContext` is scoped.** The UI host replaces `HttpTenantProvider` with `BlazorTenantProvider` because SignalR-driven Blazor interactions cannot rely on `HttpContext`.
- **Application requests are `record` types** implementing `IRequest<T>`. Handlers and validators are `sealed` and co-located with their request inside the aggregate feature folders.
- **Validation** is centralized through `Modules.Records.Application/Common/Behaviors/ValidationBehavior.cs`. Add FluentValidation validators rather than duplicating validation in handlers or UI services.
- **Aggregate creation** goes through domain factories (`IncidentFactory`, `ArrestFactory`, etc.). Constructors are intentionally restricted; private parameterless constructors are reserved for EF materialization.
- **`AppDbContext.SaveChangesAsync`** persists domain events into the outbox and dispatches them in-process so projections update immediately. Read-model handlers must be idempotent because rebuild/replay paths exist.
- **Soft delete and tenant isolation** are enforced via EF Core global query filters. Use `IgnoreQueryFilters()` only when a test or admin flow truly needs deleted or cross-tenant data.
- **Pessimistic record locking** is a first-class feature. Check aggregate methods before bypassing lock ownership or lifecycle rules in handlers or UI flows.

## Important files to read before deeper changes

- `ARCHITECTURE.md`
- `.github/workflows/deploy.yml`
- `Modules.Records.UI/Program.cs`
- `Shared.Infrastructure/DependencyInjection.cs`
- `Modules.Records.Application/Common/Behaviors/ValidationBehavior.cs`

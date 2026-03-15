# Authority.Records Copilot Instructions

Use the solution root as the working directory.

## Build, test, and lint commands

There is no dedicated lint command documented in this repository today. Use the existing .NET build and test commands instead.

```powershell
dotnet restore
dotnet build .\Authority.Records.sln --no-restore -c Release
dotnet test .\Authority.Records.sln --no-build -c Release
```

Run an individual test project for faster feedback:

```powershell
dotnet test .\Modules.Records.Domain.Tests\Modules.Records.Domain.Tests.csproj
dotnet test .\Infrastructure.IntegrationTests\Infrastructure.IntegrationTests.csproj
dotnet test .\Api.Tests\Api.Tests.csproj
dotnet test .\Modules.Records.Application.Tests\Modules.Records.Application.Tests.csproj
```

Run a single test with `FullyQualifiedName` filtering. This is more reliable here than category-based filtering because the CI workflow excludes integration tests with `Category!=Integration`, but the test sources do not currently define matching xUnit traits.

```powershell
dotnet test .\Modules.Records.Domain.Tests\Modules.Records.Domain.Tests.csproj --filter "FullyQualifiedName~Modules.Records.Domain.Tests.Entities.NameTests.Constructor_CreatesPerson_WithAllRequiredFields"
dotnet test .\Infrastructure.IntegrationTests\Infrastructure.IntegrationTests.csproj --filter "FullyQualifiedName~Infrastructure.IntegrationTests.SoftDelete.SoftDeleteTests.Should_Filter_Out_SoftDeleted_Records"
```

The deployment workflow currently uses:

```powershell
dotnet restore
dotnet build --no-restore -c Release
dotnet test --no-build -c Release --filter "Category!=Integration"
```

Check `.github/workflows/deploy.yml` before changing build, migration, publish, or deployment behavior.

## High-level architecture

This is a layered .NET 9 application built around a Blazor Server UI, a CQRS application layer, a domain model, and shared infrastructure. Read `ARCHITECTURE.md` before making large cross-cutting changes.

- `Modules.Records.UI` is the main host. It registers `AddApplication()` and `AddInfrastructure(...)`, overrides `ITenantProvider` with the Blazor-aware implementation, and uses scoped UI services as thin wrappers over MediatR.
- `Modules.Records.Application` contains the use cases. Features are organized by aggregate area such as `Incidents`, `Arrests`, and `Citations`, with MediatR requests, handlers, and FluentValidation validators.
- `Modules.Records.Domain` contains the aggregates, factories, state/lifecycle rules, soft-delete behavior, optimistic concurrency, and domain events.
- `Shared.Infrastructure` owns EF Core persistence, Identity, tenant resolution, audit handling, locking cleanup, outbox processing, and read-model rebuild/background services.
- `Api` is a lightweight worker-style host for infrastructure processing such as the outbox processor; it is not the primary application boundary for the UI.

The effective flow for core record workflows is:

UI service -> MediatR command/query -> domain aggregate(s) via `IApplicationDbContext` and factories -> EF Core persistence -> domain event dispatch -> read-model/projector updates

## Key repository conventions

- The UI does not use backend HTTP endpoints for the core records workflow. Preserve the existing pattern in `Modules.Records.UI\Services\*.cs`, where UI services call MediatR commands and queries directly.
- `AppDbContext` is intentionally registered as **transient** in `Shared.Infrastructure\DependencyInjection.cs` so each MediatR handler gets its own EF Core context. Do not casually change this to scoped; it is there to avoid Blazor Server circuit concurrency issues.
- `AuthDbContext` remains scoped, and the UI host replaces `HttpTenantProvider` with `BlazorTenantProvider` because SignalR-driven Blazor interactions cannot rely on `HttpContext`.
- Application requests are `record` types implementing `IRequest<T>`. Handlers and validators are typically `sealed` classes placed alongside the request within the aggregate feature folders.
- Validation is centralized through the MediatR pipeline in `Modules.Records.Application\Common\Behaviors\ValidationBehavior.cs`. Prefer adding FluentValidation validators instead of duplicating request validation in UI services or handlers.
- Aggregates are created through domain factories such as `IncidentFactory` and `ArrestFactory`. Constructors are intentionally restricted, with private parameterless constructors reserved for EF materialization.
- Record modifications often go through shared locking and lifecycle/state primitives. Check aggregate methods before bypassing lock ownership or lifecycle rules in handlers or UI flows.
- `AppDbContext.SaveChangesAsync` persists domain events into the outbox and also dispatches them in-process so projections update immediately. Read-model handlers must remain idempotent because rebuild and replay paths exist.
- Scheduled read-model rebuilds run in infrastructure background services and can repopulate projections from aggregate tables. If persisted data looks correct before restart but wrong after restart, inspect the rebuild path as well as the write path.
- EF Core global query filters enforce soft delete and tenant isolation. Use explicit opt-outs such as `IgnoreQueryFilters()` only when a test or admin flow truly needs deleted or cross-tenant data.
- Tests use xUnit. Domain tests usually construct aggregates directly and assert on both state and raised domain events, while integration tests use in-memory SQLite with real DI wiring rather than heavy mocking.

## Important files to read first for deeper changes

- `ARCHITECTURE.md`
- `.github/workflows/deploy.yml`
- `Modules.Records.UI\Program.cs`
- `Shared.Infrastructure\DependencyInjection.cs`
- `Modules.Records.Application\AssemblyReference.cs`
- `Modules.Records.Application\Common\Behaviors\ValidationBehavior.cs`

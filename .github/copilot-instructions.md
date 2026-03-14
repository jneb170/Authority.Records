# Authority.Records Copilot Instructions

## Build and test commands

Use the solution root as the working directory.

```powershell
dotnet restore
dotnet build .\Authority.Records.sln --no-restore -c Release
dotnet test .\Authority.Records.sln --no-build -c Release
```

Run an individual test project when you want faster feedback:

```powershell
dotnet test .\Modules.Records.Domain.Tests\Modules.Records.Domain.Tests.csproj
dotnet test .\Infrastructure.IntegrationTests\Infrastructure.IntegrationTests.csproj
dotnet test .\Api.Tests\Api.Tests.csproj
dotnet test .\Modules.Records.Application.Tests\Modules.Records.Application.Tests.csproj
```

Run a single test with `FullyQualifiedName` filtering. This is more reliable here than category-based filtering because the workflow uses `Category!=Integration`, but the test sources do not currently define matching xUnit traits.

```powershell
dotnet test .\Modules.Records.Domain.Tests\Modules.Records.Domain.Tests.csproj --filter "FullyQualifiedName~Modules.Records.Domain.Tests.Entities.NameTests.Constructor_CreatesPerson_WithAllRequiredFields"
dotnet test .\Infrastructure.IntegrationTests\Infrastructure.IntegrationTests.csproj --filter "FullyQualifiedName~Infrastructure.IntegrationTests.SoftDelete.SoftDeleteTests.Should_Filter_Out_SoftDeleted_Records"
```

CI builds with:

```powershell
dotnet restore
dotnet build --no-restore -c Release
dotnet test --no-build -c Release --filter "Category!=Integration"
```

See `.github/workflows/deploy.yml` before changing build, migration, publish, or deployment behavior.

## High-level architecture

This solution is a layered .NET 9 application with a Blazor Server UI, a CQRS application layer, a domain model, and shared infrastructure. `ARCHITECTURE.md` is the best overview and should be read before large changes.

- `Modules.Records.Domain` is the core model. Aggregate roots inherit from shared primitives that add domain events, lifecycle/state transitions, soft delete support, optimistic concurrency, and locking behavior.
- `Modules.Records.Application` contains feature folders organized by aggregate (`Incidents`, `Arrests`, `Citations`). Each feature uses MediatR request/handler pairs plus FluentValidation validators.
- `Shared.Infrastructure` owns EF Core persistence, Identity, tenant resolution, outbox processing, audit support, lock cleanup, and read-model rebuild/background services.
- `Modules.Records.UI` is the primary app host. It registers `AddApplication()` and `AddInfrastructure(...)`, overrides `ITenantProvider` with the Blazor-specific implementation, and uses scoped UI services as thin wrappers around MediatR.
- `Api` is not a REST API surface for the UI; it is a lightweight host for worker-style infrastructure concerns such as `OutboxProcessor`.

The effective flow is UI service -> MediatR command/query -> domain aggregate(s) through `IApplicationDbContext`/factories -> EF Core persistence -> domain event projection handlers/read models.

## Key repository conventions

- The UI does not talk to backend HTTP endpoints for core records workflows. In `Modules.Records.UI\Services\*.cs`, services call MediatR directly with commands and queries. Preserve that pattern unless you are intentionally introducing a new boundary.
- `AppDbContext` is intentionally registered as **transient** in `Shared.Infrastructure\DependencyInjection.cs` so each MediatR handler gets its own EF Core context. Do not casually change this to scoped; the current setup is there to avoid Blazor Server circuit concurrency problems.
- `AuthDbContext` stays scoped, and `ITenantProvider` is overridden in the UI host with `BlazorTenantProvider` because SignalR interactions cannot rely on `HttpContext`.
- Commands and queries are `record` types implementing `IRequest<T>`. Handlers and validators are typically `sealed` classes. When adding a use case, follow the existing feature-folder shape: `Commands\<Feature>\`, `Queries\<Feature>\`, validator beside the request, handler beside the request.
- Validation is centralized through MediatR pipeline behavior (`ValidationBehavior<,>`). Prefer adding FluentValidation validators instead of duplicating request validation in UI services.
- Aggregates are created via domain factories such as `IncidentFactory` and `ArrestFactory`. Constructors are intentionally restricted, and entities use private parameterless constructors for EF materialization. Preserve that pattern when adding domain entities.
- Incidents and arrests use the shared locking/state machine primitives; changes that modify records often require lock ownership. Check aggregate methods before bypassing locking rules in handlers or UI flows.
- `Incident`, `Arrest`, and related aggregates raise domain events during state changes. `AppDbContext.SaveChangesAsync` writes those events to the outbox and then dispatches them in-process for immediate projection updates. Projection handlers must stay idempotent.
- EF Core uses global query filters for soft delete and tenant isolation. If you need deleted records or cross-tenant inspection in tests/admin flows, you will need explicit opt-outs such as `IgnoreQueryFilters()`.
- Tests use xUnit. Domain tests tend to instantiate aggregates directly and assert both state and emitted domain events. Integration tests use in-memory SQLite and real DI/service-provider wiring rather than heavy mocking.

## Important files to check before deep changes

- `ARCHITECTURE.md`
- `.github/workflows/deploy.yml`
- `Shared.Infrastructure\DependencyInjection.cs`
- `Modules.Records.UI\Program.cs`
- `Modules.Records.Application\AssemblyReference.cs`
- `Modules.Records.Application\Common\Behaviors\ValidationBehavior.cs`




## Copilot Instructions for Code Review

When performing code review in this repository, prioritize finding real defects, regressions, security issues, maintainability risks, and violations of established architecture. Avoid style-only feedback unless it affects correctness, readability, or long-term reliability.

Review Priorities

Focus review comments on:

 - correctness and behavioral regressions
 - security issues, data exposure, auth/authz mistakes, and unsafe input handling
 - performance problems that are likely to matter in production
 - broken async patterns, cancellation issues, and deadlock risks
 - nullability and exception-handling problems
 - API contract changes and backward compatibility concerns
 - Blazor rendering, lifecycle, and state management bugs
 - EF Core query correctness, tracking behavior, and N+1 issues
 - dependency injection misuse and lifetime mismatches
 - logging, configuration, and observability gaps that make incidents harder to diagnose

Do not leave comments that only restate what the code does. Prefer comments that explain why something is risky and
what change would reduce the risk.

General .NET Expectations

Prefer established .NET and repository conventions over novel patterns.

Check for:

 - correct use of async/await
 - no sync-over-async (.Result, .Wait(), blocking on tasks)
 - proper propagation of CancellationToken where appropriate
 - explicit handling of nullable reference types
 - avoiding broad exception swallowing
 - clear error paths instead of silent fallback behavior
 - correct DI registration and appropriate service lifetimes
 - no hidden global state unless clearly intended
 - configuration bound safely and validated when needed
 - structured logging with useful context, but without leaking secrets or personal data

C# Review Guidance

Pay attention to:

 - incorrect null assumptions
 - mutation of shared state across threads or requests
 - LINQ that is hard to reason about or causes repeated enumeration
 - misuse of IDisposable / IAsyncDisposable
 - leaking implementation details through public APIs
 - overuse of static helpers where injected services would be safer/testable
 - incorrect equality, culture, timezone, or string-comparison behavior
 - accidental allocations or heavy work in hot paths

Prefer actionable suggestions that align with modern C# and the existing codebase patterns.

ASP.NET Core / Backend Guidance

Check for:

 - missing authorization on sensitive endpoints
 - model binding assumptions that allow invalid or partial input
 - inconsistent validation between UI and server
 - unsafe file, path, or serialization behavior
 - improper use of HttpClient
 - missing timeouts, retries, or resilience where external calls are involved
 - endpoint behavior changes that could break consumers
 - missing audit/logging for important business actions
 - improper use of scoped services from singleton dependencies

If a change touches APIs, verify:

 - response shape compatibility
 - status code correctness
 - validation and error response consistency
 - versioning implications
 - contract/documentation drift

EF Core / Data Access Guidance

Review for:

 - N+1 queries
 - loading too much data into memory
 - client-side evaluation where server-side execution is expected
 - incorrect tracking vs. no-tracking behavior
 - transaction boundaries that are too broad or missing
 - concurrency issues and missing handling for conflicting updates
 - migrations that may be destructive or unsafe for production rollout
 - brittle raw SQL usage or SQL injection risks
 - missing indexes implied by new query patterns

Prefer comments that identify the concrete query or data-shape risk.

Blazor-Specific Guidance

Review components for:

 - misuse of lifecycle methods such as OnInitialized{Async}, OnParametersSet{Async}, and OnAfterRender{Async}
 - infinite render loops or unnecessary re-renders
 - state updates that may happen off the correct flow
 - incorrect assumptions about prerendering or circuit lifetime
 - missing disposal of event handlers, timers, and JS object references
 - JS interop calls that can fail during prerendering or after disposal
 - parameter mutation or hidden coupling between parent and child components
 - forms and validation behavior that differs from user expectations
 - accessibility regressions in interactive components
 - large components that mix rendering, business logic, and data access in ways that reduce testability

For Blazor Server, also watch for:

 - circuit-related state problems
 - long-running or blocking work on the UI path
 - over-chatty UI updates
 - unintended shared state between users

For Blazor WebAssembly, also watch for:

 - unnecessary payload growth
 - sensitive logic or secrets pushed client-side
 - API assumptions that fail under latency or offline conditions

Security Guidance

Always flag:

 - secrets, tokens, or connection strings in code or logs
 - missing authorization checks
 - trust in client-provided data
 - XSS risks in rendered content
 - insecure direct object reference patterns
 - unsafe deserialization
 - insufficient validation on file uploads or external input
 - PII leakage in telemetry, exceptions, or logs

Be especially careful when changes touch authentication, roles, claims, policies, or tenant boundaries.

Testing Expectations

When reviewing changes, consider whether tests cover:

 - happy path and important failure paths
 - null / invalid / unauthorized inputs
 - regression-prone branching logic
 - component behavior for loading, empty, error, and success states
 - API contract expectations
 - data access edge cases
 - bug fixes that should have a regression test

If tests are missing for risky logic, call that out.

Comment Style

Review comments should:

 - be specific and technical
 - explain the user or system impact
 - suggest a safer direction when possible
 - avoid purely stylistic nitpicks unless they hide a bug
 - avoid requesting unnecessary rewrites

Good review comments identify:

 1. what is wrong
 2. why it matters
 3. what change would make it safer or clearer

Severity Heuristics

Treat these as high priority:

 - security vulnerabilities
 - data loss or corruption risks
 - auth/authz bypass
 - race conditions and concurrency bugs
 - breaking API or schema changes
 - likely production exceptions
 - serious Blazor lifecycle or disposal bugs
 - EF Core query patterns that will degrade badly at scale

Treat these as medium priority:

 - maintainability issues likely to cause future defects
 - missing cancellation or resilience in important paths
 - incomplete observability for critical operations
 - test gaps around risky business logic

Treat these as low priority:

 - minor readability concerns
 - naming or formatting issues without behavioral impact

Repository Conventions

Follow the existing architecture, naming, dependency boundaries, and project conventions in this repository. Prefer
consistency with surrounding code over generic recommendations.

If a change appears intentional but risky, explain the tradeoff and confirm whether the pattern is used consistently
elsewhere before recommending a different approach.
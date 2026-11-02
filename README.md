# Authority Records

A records management system (RMS) for law-enforcement agencies: incident reports, arrests and citations, plus the people and places connected to them.

**Live demo:** <https://authorityrecords.dev>. Click **Explore the live demo** to sign in as a supervisor at a fictional agency. No account is needed, and every person, address and record in the demo is made up.

## What it does

- **Incidents, arrests and citations.** Each has its own workflow, linked charges and free-text narratives. Records move through a lifecycle (Draft → Open → Closed → Archived) with rules for each step. For example, an incident can't close while its arrests are still open or its citations unissued.
- **Master Name and Location indexes.** A person or address is entered once and linked from every record it appears in, so a subject's history is one click away.
- **Pessimistic record locking.** Only one person can edit a record at a time. Locks renew while you work and expire when you leave, so a record is never stuck.
- **Multi-agency tenancy.** Jurisdictions and agencies are isolated with EF Core global query filters. Users can belong to several agencies and switch between them.
- **Role-based access.** Super, Admin, Supervisor, Officer and Dispatcher roles, each with a full audit trail.
- **Texas citation printing.** The official citation form is rendered as a server-generated PDF (QuestPDF) that matches the State layout.
- **Admin tooling.** Charge catalog (UCR/NIBRS), configurable picklists, hotkeys, read-model rebuilds, logs, and a fake-data generator.

## Architecture

```
Browser ──(Blazor Server circuit)──▶ Modules.Records.UI
                                      │  thin scoped services
                                      ▼
                                 MediatR pipeline ── validation, demo rate limits
                                      │
                                      ▼
                          Modules.Records.Application  (CQRS commands / queries)
                                      │
                                      ▼
                           Modules.Records.Domain  (aggregates, factories, policies, events)
                                      │
                                      ▼
                    Shared.Infrastructure  (EF Core, Identity, tenancy, outbox, read models)
```

- **Domain-driven design.** Aggregates (`Incident`, `Arrest`, `Citation`, `Name`, `Location`) are created through factories and protect their own invariants: lifecycle transitions, lock ownership, soft delete and optimistic concurrency. They raise domain events for every change.
- **CQRS with MediatR.** Every use case is a `record` request with a co-located handler and FluentValidation validator. Cross-cutting concerns such as validation and abuse limits are pipeline behaviors.
- **Transactional outbox and read models.** `SaveChangesAsync` writes domain events to an outbox in the same transaction and dispatches them in-process, so projections update immediately. Background services retry failures and can rebuild projections from scratch.
- **No HTTP API for the core workflow.** The Blazor Server UI calls MediatR directly. Each handler gets its own transient `DbContext` to stay safe across concurrent circuit events.

[`ARCHITECTURE.md`](ARCHITECTURE.md) has the full component reference.

## Tech stack

.NET 10 · Blazor Server (Razor Components, Interactive Server) · EF Core (SQLite in production, SQL Server supported) · ASP.NET Core Identity · MediatR · FluentValidation · QuestPDF · xUnit · Docker · Caddy · AWS Lightsail · GitHub Actions

## Running locally

Requires the .NET 10 SDK.

```bash
DefaultDatabaseProvider=Sqlite dotnet run --project Modules.Records.UI
```

This uses local SQLite files. Leave out `DefaultDatabaseProvider` to use the SQL Server connection in `appsettings`. Either way, migrations apply on startup, and the demo agency, demo users and sample dataset are seeded automatically.

Or run the production container:

```bash
docker build -t authority-records .
```

```bash
docker run --rm -p 8080:8080 authority-records
```

Then open <http://localhost:8080>.

## Tests

```bash
dotnet test ./Authority.Records.sln
```

- **Domain tests** construct aggregates directly and assert on both state and raised events.
- **Application tests** cover handlers and validators.
- **Integration tests** run the real DI container against in-memory SQLite.

## Deployment

Every push to `master` builds and tests the solution, publishes a container image to GHCR, and deploys over SSH to a single AWS Lightsail instance. The instance runs Docker Compose with Caddy, which handles automatic HTTPS. Databases, Data Protection keys and uploads live on Docker volumes, and the instance is snapshotted daily. See [`Documents/aws-deployment-guide.md`](Documents/aws-deployment-guide.md).

## About the demo

Each "Explore the live demo" click signs in a separate visitor account (for example "Demo Visitor 4821", a Supervisor at the demo agency), so record locking, the audit trail and create quotas are per visitor. Open a record in two browsers and press Modify in both to see locking. Each visitor can create 30 records per hour, all visitors together 600 per hour, and a save is capped at 64 KB, so the demo can't be used to fill the server. Visitor accounts are deleted after 14 days and can't change their password. Demo data is seeded once per agency by [`DemoDataset`](Modules.Records.UI/Demo/DemoDataset.cs). It goes through the same MediatR commands a user would trigger, so it passes the same validation, domain rules and projections.

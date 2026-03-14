# Feature Specification Template

> Copy this file, rename it to describe your feature (e.g., `feature-citation-export.md`), and fill in each section.

---

## Feature Title

_A short, clear name for the feature._

---

## Summary

_One or two sentences describing what this feature does and why it matters._

---

## Motivation / Problem Statement

_What user pain point, workflow gap, or business need does this address? Who benefits?_

---

## Affected Domain Areas

_Check all that apply and describe the impact briefly._

- [ ] **Incident** aggregate — _e.g., new state transition, new field, new behavior_
- [ ] **Arrest** aggregate — _describe_
- [ ] **Citation** aggregate — _describe_
- [ ] **Name / Person** entity — _describe_
- [ ] **Jurisdiction / Agency configuration** — _describe_
- [ ] **Picklist / Settings** — _describe_
- [ ] **Identity / Users** — _describe_
- [ ] **Cross-cutting / Infrastructure** — _describe_

---

## Domain Changes

### New / Modified Entities or Value Objects

| Entity / Value Object | Change Type | Description |
|-----------------------|-------------|-------------|
| `Incident`            | Modified    | Add `X` property |
| `NewValueObject`      | New         | Encapsulates ... |

### New / Modified Domain Events

| Event | Trigger |
|-------|---------|
| `IncidentXyzDomainEvent` | Raised when ... |

### Business Rules / Invariants

_List any new or changed rules the domain must enforce._

- Rule 1: ...
- Rule 2: ...

---

## Application Layer (CQRS)

### New Commands

| Command | Handler | Description |
|---------|---------|-------------|
| `DoSomethingCommand` | `DoSomethingCommandHandler` | ... |

### New Queries

| Query | Handler | Returns |
|-------|---------|---------|
| `GetSomethingQuery` | `GetSomethingQueryHandler` | `SomethingDto` |

### Validation Rules (FluentValidation)

_List key validation rules for new commands._

- `DoSomethingCommand.FieldX` — must not be empty, max 200 chars
- ...

---

## UI / Blazor Changes

### Affected Pages / Components

| Page / Component | Change |
|-----------------|--------|
| `Incidents/Detail.razor` | Add button for ... |
| `NewFeature.razor` | New page — accessible at `/new-feature` |

### User Workflow

_Describe the step-by-step interaction from the user's perspective._

1. User navigates to ...
2. User clicks ...
3. System responds with ...

---

## API Changes (if applicable)

| Method | Route | Request | Response |
|--------|-------|---------|----------|
| `POST` | `/api/incidents/{id}/xyz` | `XyzRequest` | `XyzResponse` |

---

## Infrastructure / Data Changes

### Database / EF Core

- [ ] New table(s): _list_
- [ ] New column(s): _table.column, type_
- [ ] New migration required: Yes / No
- [ ] Read model projection changes: _describe_

### Multi-tenancy

_Does this feature need special handling for `JurisdictionId` or `AgencyId` scoping?_

---

## Authorization & Permissions

_Who can use this feature? Are there role/policy checks needed?_

- Required role(s): _e.g., Officer, Supervisor, Admin_
- New policy needed: Yes / No — _describe if yes_
- Respects existing soft-delete / record-lock rules: Yes / No / N/A

---

## Record Locking Considerations

_Does this feature read or modify lockable aggregates (Incident, Arrest, Citation)?_

- Requires acquire-lock before edit: Yes / No
- Works on locked records (read-only): Yes / No
- Bypasses lock (admin operation): Yes / No — _justify if yes_

---

## Outbox / Domain Event Handling

_Are any domain events raised that need outbox delivery or downstream handlers?_

| Event | Handler(s) | Side Effect |
|-------|-----------|-------------|
| `IncidentXyzDomainEvent` | `UpdateReadModelHandler` | Updates `IncidentReadModel` |

---

## Testing Plan

### Unit Tests (`Modules.Records.Domain.Tests`)

- [ ] Test: _domain rule / behavior_
- [ ] Test: _edge case_

### Application Tests (`Modules.Records.Application.Tests`)

- [ ] Test: command succeeds with valid input
- [ ] Test: command fails validation
- [ ] Test: query returns expected results

### Integration Tests (`Infrastructure.IntegrationTests`)

- [ ] Test: end-to-end against real DB (if warranted)

---

## Acceptance Criteria

_Define "done." Each criterion should be verifiable._

- [ ] A user with role X can ...
- [ ] The system prevents ... when ...
- [ ] An audit trail entry is created when ...
- [ ] The read model reflects changes within ...

---

## Out of Scope

_Explicitly list what this feature does NOT include to prevent scope creep._

- ...

---

## Open Questions

_Unresolved decisions that need input before implementation._

| # | Question | Owner | Status |
|---|----------|-------|--------|
| 1 | Should this affect archived records? | | Open |

---

## Notes / References

- Related blueprint: `Documents/records-module-blueprint.md`
- Related architecture doc: `ARCHITECTURE.md`
- Related issue / PR: _link_

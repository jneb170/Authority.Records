# Feature Specification 

## Feature Title

Add Location Module

---

## Summary

This will add a new module to Authority called Location which will store the address information of a given location. The location records can be used in other modules to specify the address of an incident/arrest/citation, primary/secondary addresses of Names, Court Addresses (not created yet) and many more.

---

## Motivation / Problem Statement

Users need access to a Master Location Index.

---

## Affected Domain Areas

_Check all that apply and describe the impact briefly._

- [X] **Incident** aggregate — a new field will to be added to store the Incident's Location
- [X] **Arrest** aggregate — a new field will to be added to store the Arrest's Location
- [X] **Citation** aggregate — a new field will to be added to store the Citation's Location
- [X] **Name / Person** entity — 2 new fields will need to be added to store the Name's primary and secondary address.
- [X] **Jurisdiction / Agency configuration** — locations can be shared across all agencies in a juridiction but not outside the jurisdiction. Records related to the location are only visible to user's of that agency.
- [X] **Picklist / Settings** — any fields that require a picklist in the new location module will need to be defined in the Picklist/Settings.
- [ ] **Identity / Users** — _describe_
- [ ] **Cross-cutting / Infrastructure** — _describe_

---

## Domain Changes

### New / Modified Entities or Value Objects

| Entity / Value Object | Change Type | Description |
|-----------------------|-------------|-------------|
| `Location`      | New         | Encapsulates Location information |

### New / Modified Domain Events

| Event | Trigger |
|-------|---------|
| `LocationCreatedDomainEvent` | Raised when Location Record is created |
| `LocationUpdatedDomainEvent` | Raised when Location Record is updated |
| `LocationRestoredDomainEvent` | Raised when Location Record is restored |
| `LocationSoftDeleteDomainEvent` | Raised when Location Record is soft deleted |

### Business Rules / Invariants

Rule 1: Other modules will reference the Location record. Location will be a one to many relationship.
Rule 2: Other modules will use a common Location object that:
	-Displays the location information
	-Has a search icon that opens a search screen to either find or add a location record.
	-The location cannot be directly updated from other modules. They can only be added through the search screen.
	-Users can start typing a location and the Location object should auto-suggest matching locations.
Rule 3: Location field on other modules will display the address information of the location in a standard or custom format. The standard format will be Street Number + Predirection + Street Address + Street Type + Postdirection + City + State. For example: "123 S Main St NW Springfield, IL".
The custom format can be set at the Jurisdiction Level and can be specific to an individual Module/Field combination. For example: the citation location may only show the street name of the location on the module.
Rule 4: The application will need the ability to show the location in an embedded Google Map.

---

## Application Layer (CQRS)

### New Commands

Reference the Names Entity for a list of commands that will be needed for Location.

### New Queries

Reference the Names Entity for a list of queries that will be needed for Location.

## UI / Blazor Changes

### Affected Pages / Components

| Page / Component | Change |
|-----------------|--------|
| incident/arrest/citation/name | Add Incident Location field |
| arrest | Add Arrest Location field |
| citation | Add Citation Location field |
| name | Add Primary Address field |
| name | Add Secondary Address field |

---

## Infrastructure / Data Changes

### Database / EF Core

- [X] New table(s): Location
- [X] New column(s): 
		Street Number - string
		Predirection - picklist of type=DIR
		Street Address - string (with proper formatting)
		Street Type - picklist of type= STTYPE
		Postdirection - picklist of type=DIR
		City - string
		State - picklist (use existing STATES)
		Country - picklist of type=COUNTRY
		Zip - string
		Apartment / Suite # - string
		Coordinates - string
		Common Place Name - string
		Comments - string

- [X] New migration required: Yes 
- [X] Read model projection changes: Add to read model using Name read model as an example.

### Multi-tenancy

Location Records are available to all agencies in a juridiction/tenant. They are not available to other tenants. Any non-location records linked to the location should not be accessible if in an agency that the user does not belong to.

---

## Authorization & Permissions

- Respects existing soft-delete / record-lock rules: Yes 

---

## Record Locking Considerations

- Does not update other modules so no lock required when adding or modifying from the Location module.
- When creating/linking a location from another module's location object, the non-location module will be in create or modify mode and unlocked.
- Requires acquire-lock before edit: Yes

---

## Outbox / Domain Event Handling

Reference the Names Entity for a list of Outbox/Domain Event Handling that will be needed for Location.

---

## Notes / References

- Related blueprint: `Documents/records-module-blueprint.md`
- Related architecture doc: `ARCHITECTURE.md`

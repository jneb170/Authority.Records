using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Relationships.Queries.GetRecordRelationships;

public sealed class GetRecordRelationshipsHandler(
    IApplicationDbContext dbContext,
    ITenantProvider tenantProvider)
    : IRequestHandler<GetRecordRelationshipsQuery, RecordRelationshipsDto?>
{
    public Task<RecordRelationshipsDto?> Handle(
        GetRecordRelationshipsQuery request,
        CancellationToken cancellationToken)
    {
        var jurisdictionId = tenantProvider.GetJurisdictionId();

        return request.RecordType switch
        {
            RecordRelationshipRecordTypes.Incident => LoadIncidentAsync(jurisdictionId, request.RecordNumber, cancellationToken),
            RecordRelationshipRecordTypes.Arrest => LoadArrestAsync(jurisdictionId, request.RecordNumber, cancellationToken),
            RecordRelationshipRecordTypes.Citation => LoadCitationAsync(jurisdictionId, request.RecordNumber, cancellationToken),
            RecordRelationshipRecordTypes.Name => LoadNameAsync(jurisdictionId, request.RecordNumber, cancellationToken),
            RecordRelationshipRecordTypes.Location => LoadLocationAsync(jurisdictionId, request.RecordNumber, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request.RecordType), request.RecordType, "Unsupported record type.")
        };
    }

    private async Task<RecordRelationshipsDto?> LoadIncidentAsync(
        Guid jurisdictionId,
        long recordNumber,
        CancellationToken cancellationToken)
    {
        var incident = await dbContext.IncidentReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.JurisdictionId == jurisdictionId && x.RecordNumber == recordNumber,
                cancellationToken);

        if (incident is null)
            return null;

        var groups = new List<RecordRelationshipGroupDto>();

        if (incident.LocationId.HasValue)
        {
            var location = await dbContext.LocationReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.JurisdictionId == jurisdictionId && x.Id == incident.LocationId.Value,
                    cancellationToken);

            AddGroup(groups, "Location", location is null
                ? []
                : [CreateLocationItem(location, "Incident location")]);
        }

        var arrestLinks = await dbContext.IncidentArrestLinkReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && x.IncidentId == incident.Id)
            .ToListAsync(cancellationToken);

        var arrests = await dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && arrestLinks.Select(l => l.ArrestId).Contains(x.Id))
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        var arrestNames = await LoadNamesAsync(
            jurisdictionId,
            arrests.Where(x => x.NameId.HasValue).Select(x => x.NameId!.Value),
            cancellationToken);

        AddGroup(groups, "Linked Arrests", arrests
            .Select(x => CreateArrestItem(x, GetNameValue(arrestNames, x.NameId), "Linked arrest"))
            .ToList());

        var citationLinks = await dbContext.IncidentCitationLinkReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && x.IncidentId == incident.Id)
            .ToListAsync(cancellationToken);

        var citations = await dbContext.CitationReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && citationLinks.Select(l => l.CitationId).Contains(x.Id))
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        AddGroup(groups, "Linked Citations", citations
            .Select(x => CreateCitationItem(x, "Linked citation"))
            .ToList());

        return new RecordRelationshipsDto(
            CreateSource(incident),
            groups);
    }

    private async Task<RecordRelationshipsDto?> LoadArrestAsync(
        Guid jurisdictionId,
        long recordNumber,
        CancellationToken cancellationToken)
    {
        var arrest = await dbContext.ArrestReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.JurisdictionId == jurisdictionId && x.RecordNumber == recordNumber,
                cancellationToken);

        if (arrest is null)
            return null;

        var groups = new List<RecordRelationshipGroupDto>();

        if (arrest.PrimaryIncidentId.HasValue)
        {
            var primaryIncident = await dbContext.IncidentReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.JurisdictionId == jurisdictionId && x.Id == arrest.PrimaryIncidentId.Value,
                    cancellationToken);

            AddGroup(groups, "Primary Incident", primaryIncident is null
                ? []
                : [CreateIncidentItem(primaryIncident, "Primary incident")]);
        }

        var linkedIncidents = await dbContext.IncidentArrestLinkReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && x.ArrestId == arrest.Id)
            .Join(
                dbContext.IncidentReadModels.AsNoTracking(),
                link => link.IncidentId,
                incident => incident.Id,
                (_, incident) => incident)
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        AddGroup(groups, "Linked Incidents", linkedIncidents
            .Select(x => CreateIncidentItem(x, "Linked incident"))
            .ToList());

        if (arrest.NameId.HasValue)
        {
            var name = await dbContext.NameReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.JurisdictionId == jurisdictionId && x.Id == arrest.NameId.Value,
                    cancellationToken);

            AddGroup(groups, "Suspect", name is null
                ? []
                : [CreateNameItem(name, "Suspect")]);
        }

        if (arrest.LocationId.HasValue)
        {
            var location = await dbContext.LocationReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.JurisdictionId == jurisdictionId && x.Id == arrest.LocationId.Value,
                    cancellationToken);

            AddGroup(groups, "Location", location is null
                ? []
                : [CreateLocationItem(location, "Arrest location")]);
        }

        var suspectName = arrest.NameId.HasValue
            ? GetNameValue(await LoadNamesAsync(jurisdictionId, [arrest.NameId.Value], cancellationToken), arrest.NameId)
            : null;

        return new RecordRelationshipsDto(
            CreateSource(arrest, suspectName),
            groups);
    }

    private async Task<RecordRelationshipsDto?> LoadCitationAsync(
        Guid jurisdictionId,
        long recordNumber,
        CancellationToken cancellationToken)
    {
        var citation = await dbContext.CitationReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.JurisdictionId == jurisdictionId && x.RecordNumber == recordNumber,
                cancellationToken);

        if (citation is null)
            return null;

        var groups = new List<RecordRelationshipGroupDto>();

        var linkedIncidents = await dbContext.IncidentCitationLinkReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && x.CitationId == citation.Id)
            .Join(
                dbContext.IncidentReadModels.AsNoTracking(),
                link => link.IncidentId,
                incident => incident.Id,
                (_, incident) => incident)
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        AddGroup(groups, "Linked Incidents", linkedIncidents
            .Select(x => CreateIncidentItem(x, "Linked incident"))
            .ToList());

        if (citation.LocationId.HasValue)
        {
            var location = await dbContext.LocationReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.JurisdictionId == jurisdictionId && x.Id == citation.LocationId.Value,
                    cancellationToken);

            AddGroup(groups, "Location", location is null
                ? []
                : [CreateLocationItem(location, "Citation location")]);
        }

        return new RecordRelationshipsDto(
            CreateSource(citation),
            groups);
    }

    private async Task<RecordRelationshipsDto?> LoadNameAsync(
        Guid jurisdictionId,
        long recordNumber,
        CancellationToken cancellationToken)
    {
        var name = await dbContext.NameReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.JurisdictionId == jurisdictionId && x.RecordNumber == recordNumber,
                cancellationToken);

        if (name is null)
            return null;

        var groups = new List<RecordRelationshipGroupDto>();

        var arrests = await dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && x.NameId == name.Id)
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        AddGroup(groups, "Related Arrests", arrests
            .Select(x => CreateArrestItem(x, FormatName(name), "Suspect named in arrest"))
            .ToList());

        if (name.PrimaryLocationId.HasValue)
        {
            var primaryLocation = await dbContext.LocationReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.JurisdictionId == jurisdictionId && x.Id == name.PrimaryLocationId.Value,
                    cancellationToken);

            AddGroup(groups, "Primary Location", primaryLocation is null
                ? []
                : [CreateLocationItem(primaryLocation, "Primary location")]);
        }

        if (name.SecondaryLocationId.HasValue)
        {
            var secondaryLocation = await dbContext.LocationReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.JurisdictionId == jurisdictionId && x.Id == name.SecondaryLocationId.Value,
                    cancellationToken);

            AddGroup(groups, "Secondary Location", secondaryLocation is null
                ? []
                : [CreateLocationItem(secondaryLocation, "Secondary location")]);
        }

        return new RecordRelationshipsDto(
            CreateSource(name),
            groups);
    }

    private async Task<RecordRelationshipsDto?> LoadLocationAsync(
        Guid jurisdictionId,
        long recordNumber,
        CancellationToken cancellationToken)
    {
        var location = await dbContext.LocationReadModels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.JurisdictionId == jurisdictionId && x.RecordNumber == recordNumber,
                cancellationToken);

        if (location is null)
            return null;

        var groups = new List<RecordRelationshipGroupDto>();

        var incidents = await dbContext.IncidentReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && x.LocationId == location.Id)
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        AddGroup(groups, "Related Incidents", incidents
            .Select(x => CreateIncidentItem(x, "Incident at this location"))
            .ToList());

        var arrests = await dbContext.ArrestReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && x.LocationId == location.Id)
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        var arrestNames = await LoadNamesAsync(
            jurisdictionId,
            arrests.Where(x => x.NameId.HasValue).Select(x => x.NameId!.Value),
            cancellationToken);

        AddGroup(groups, "Related Arrests", arrests
            .Select(x => CreateArrestItem(x, GetNameValue(arrestNames, x.NameId), "Arrest at this location"))
            .ToList());

        var citations = await dbContext.CitationReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && x.LocationId == location.Id)
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        AddGroup(groups, "Related Citations", citations
            .Select(x => CreateCitationItem(x, "Citation at this location"))
            .ToList());

        var names = await dbContext.NameReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId &&
                (x.PrimaryLocationId == location.Id || x.SecondaryLocationId == location.Id))
            .OrderBy(x => x.RecordNumber)
            .ToListAsync(cancellationToken);

        AddGroup(groups, "Related Names", names
            .Select(x => CreateNameItem(
                x,
                x.PrimaryLocationId == location.Id && x.SecondaryLocationId == location.Id
                    ? "Primary and secondary location"
                    : x.PrimaryLocationId == location.Id
                        ? "Primary location"
                        : "Secondary location"))
            .ToList());

        return new RecordRelationshipsDto(
            CreateSource(location),
            groups);
    }

    private async Task<Dictionary<Guid, string>> LoadNamesAsync(
        Guid jurisdictionId,
        IEnumerable<Guid> nameIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.NameReadModels
            .AsNoTracking()
            .Where(x => x.JurisdictionId == jurisdictionId && nameIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, FormatName, cancellationToken);
    }

    private static void AddGroup(
        ICollection<RecordRelationshipGroupDto> groups,
        string title,
        IReadOnlyList<RecordRelationshipItemDto> items)
    {
        if (items.Count > 0)
        {
            groups.Add(new RecordRelationshipGroupDto(title, items));
        }
    }

    private static RecordRelationshipSourceDto CreateSource(IncidentReadModel incident) =>
        new(
            RecordRelationshipRecordTypes.Incident,
            incident.RecordNumber,
            BuildIncidentTitle(incident),
            NullIfWhiteSpace(incident.Description),
            $"/incidents/{incident.RecordNumber}");

    private static RecordRelationshipSourceDto CreateSource(ArrestReadModel arrest, string? suspectName) =>
        new(
            RecordRelationshipRecordTypes.Arrest,
            arrest.RecordNumber,
            BuildArrestTitle(arrest),
            NullIfWhiteSpace(suspectName),
            $"/arrests/{arrest.RecordNumber}");

    private static RecordRelationshipSourceDto CreateSource(CitationReadModel citation) =>
        new(
            RecordRelationshipRecordTypes.Citation,
            citation.RecordNumber,
            BuildCitationTitle(citation),
            NullIfWhiteSpace(citation.Description),
            $"/citations/{citation.RecordNumber}");

    private static RecordRelationshipSourceDto CreateSource(NameReadModel name) =>
        new(
            RecordRelationshipRecordTypes.Name,
            name.RecordNumber,
            FormatName(name),
            name.NameType,
            $"/names/{name.RecordNumber}");

    private static RecordRelationshipSourceDto CreateSource(LocationReadModel location) =>
        new(
            RecordRelationshipRecordTypes.Location,
            location.RecordNumber,
            BuildLocationTitle(location),
            BuildLocationSubtitle(location),
            $"/locations/{location.RecordNumber}");

    private static RecordRelationshipItemDto CreateIncidentItem(IncidentReadModel incident, string relationshipLabel) =>
        new(
            RecordRelationshipRecordTypes.Incident,
            incident.RecordNumber,
            BuildIncidentTitle(incident),
            NullIfWhiteSpace(incident.Description),
            $"/incidents/{incident.RecordNumber}",
            relationshipLabel);

    private static RecordRelationshipItemDto CreateArrestItem(ArrestReadModel arrest, string? suspectName, string relationshipLabel) =>
        new(
            RecordRelationshipRecordTypes.Arrest,
            arrest.RecordNumber,
            BuildArrestTitle(arrest),
            NullIfWhiteSpace(suspectName),
            $"/arrests/{arrest.RecordNumber}",
            relationshipLabel);

    private static RecordRelationshipItemDto CreateCitationItem(CitationReadModel citation, string relationshipLabel) =>
        new(
            RecordRelationshipRecordTypes.Citation,
            citation.RecordNumber,
            BuildCitationTitle(citation),
            NullIfWhiteSpace(citation.Description),
            $"/citations/{citation.RecordNumber}",
            relationshipLabel);

    private static RecordRelationshipItemDto CreateNameItem(NameReadModel name, string relationshipLabel) =>
        new(
            RecordRelationshipRecordTypes.Name,
            name.RecordNumber,
            FormatName(name),
            name.NameType,
            $"/names/{name.RecordNumber}",
            relationshipLabel);

    private static RecordRelationshipItemDto CreateLocationItem(LocationReadModel location, string relationshipLabel) =>
        new(
            RecordRelationshipRecordTypes.Location,
            location.RecordNumber,
            BuildLocationTitle(location),
            BuildLocationSubtitle(location),
            $"/locations/{location.RecordNumber}",
            relationshipLabel);

    private static string BuildIncidentTitle(IncidentReadModel incident) =>
        string.IsNullOrWhiteSpace(incident.IncidentNum)
            ? $"Incident #{incident.RecordNumber}"
            : incident.IncidentNum;

    private static string BuildArrestTitle(ArrestReadModel arrest) =>
        string.IsNullOrWhiteSpace(arrest.ArrestNum)
            ? $"Arrest #{arrest.RecordNumber}"
            : arrest.ArrestNum;

    private static string BuildCitationTitle(CitationReadModel citation) =>
        string.IsNullOrWhiteSpace(citation.CitationNum)
            ? $"Citation #{citation.RecordNumber}"
            : citation.CitationNum;

    private static string FormatName(NameReadModel name)
    {
        if (name.NameType == NameTypes.Person)
        {
            var parts = new List<string> { name.LastOrBusinessName };
            if (!string.IsNullOrWhiteSpace(name.FirstName))
            {
                parts.Add($", {name.FirstName}");
            }

            if (!string.IsNullOrWhiteSpace(name.MiddleName))
            {
                parts.Add($" {name.MiddleName}");
            }

            return string.Concat(parts);
        }

        return name.LastOrBusinessName;
    }

    private static string BuildLocationTitle(LocationReadModel location) =>
        !string.IsNullOrWhiteSpace(location.CommonPlaceName)
            ? location.CommonPlaceName
            : !string.IsNullOrWhiteSpace(location.Address)
                ? location.Address
                : $"{location.StreetAddress}, {location.City}";

    private static string? BuildLocationSubtitle(LocationReadModel location)
    {
        var address = !string.IsNullOrWhiteSpace(location.Address)
            ? location.Address
            : $"{location.StreetAddress}, {location.City}";

        return NullIfWhiteSpace(address == BuildLocationTitle(location) ? location.City : address);
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? GetNameValue(IReadOnlyDictionary<Guid, string> names, Guid? nameId) =>
        nameId.HasValue && names.TryGetValue(nameId.Value, out var name)
            ? name
            : null;
}

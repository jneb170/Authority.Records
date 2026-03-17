using System.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Admin.Commands.RebuildReadModels;

public sealed class RebuildReadModelsHandler
    : IRequestHandler<RebuildReadModelsCommand, RebuildReadModelsResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public RebuildReadModelsHandler(IApplicationDbContext db, ITenantProvider tenantProvider)
    {
        _db             = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<RebuildReadModelsResult> Handle(
        RebuildReadModelsCommand request,
        CancellationToken cancellationToken)
    {
        var sw  = Stopwatch.StartNew();
        var jid = _tenantProvider.GetJurisdictionId();

        // ── 1. Clear all read model rows for this jurisdiction ──────────────
        await _db.NameReadModels
            .Where(r => r.JurisdictionId == jid)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.ArrestReadModels
            .Where(r => r.JurisdictionId == jid)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.CitationReadModels
            .Where(r => r.JurisdictionId == jid)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.IncidentReadModels
            .Where(r => r.JurisdictionId == jid)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.IncidentArrestLinkReadModels
            .Where(r => r.JurisdictionId == jid)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.IncidentCitationLinkReadModels
            .Where(r => r.JurisdictionId == jid)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.MugshotLinkReadModels
            .Where(r => r.JurisdictionId == jid)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.MugshotReadModels
            .Where(r => r.JurisdictionId == jid)
            .ExecuteDeleteAsync(cancellationToken);

        // ── 2. Rebuild Names (global filter excludes soft-deleted) ───────────
        var names = await _db.Names
            .AsNoTracking()
            .Where(n => n.JurisdictionId == jid)
            .ToListAsync(cancellationToken);
        var nameById = names.ToDictionary(n => n.Id);

        var nameReadModels = names.Select(n =>
        {
            var rm = NameReadModel.Create(
                id:                    n.Id,
                recordNumber:          n.RecordNumber,
                jurisdictionId:        n.JurisdictionId,
                agencyId:              n.AgencyId,
                nameType:              n.NameType,
                lastOrBusinessName:    n.LastOrBusinessName,
                firstName:             n.FirstName,
                middleName:            n.MiddleName,
                sexId:                 n.SexId,
                raceId:                n.RaceId,
                dateOfBirth:           n.DateOfBirth,
                driversLicenseNumber:  n.DriversLicenseNumber,
                driversLicenseStateId: n.DriversLicenseStateId,
                heightInches:          n.HeightInches,
                weightLbs:             n.WeightLbs,
                hairColorId:           n.HairColorId,
                eyeColorId:            n.EyeColorId,
                suffixId:              n.SuffixId,
                placeOfBirth:          n.PlaceOfBirth,
                fbiNumber:             n.FbiNumber,
                localNumber:           n.LocalNumber,
                socialSecurityNumber:  n.SocialSecurityNumber,
                isCitizen:             n.IsCitizen,
                 deceasedDate:          n.DeceasedDate,
                 createdAtUtc:          n.CreatedAt,
                 createdBy:             n.CreatedBy);
            rm.ApplyLocationChanged(n.PrimaryLocationId, n.SecondaryLocationId);
            rm.ApplyModifiedAudit(n.ModifiedBy, n.ModifiedAt, n.CreatedAt);
            return rm;
        }).ToList();

        _db.NameReadModels.AddRange(nameReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        // ── 3. Rebuild Arrests ───────────────────────────────────────────────
        var arrests = await _db.Arrests
            .AsNoTracking()
            .Where(a => a.JurisdictionId == jid)
            .ToListAsync(cancellationToken);
        var arrestById = arrests.ToDictionary(a => a.Id);

        var arrestReadModels = arrests.Select(a =>
        {
            var rm = ArrestReadModel.Create(
                id:             a.Id,
                recordNumber:   a.RecordNumber,
                jurisdictionId: a.JurisdictionId,
                agencyId:       a.AgencyId,
                nameId:         a.NameId,
                arrestedAt:     a.ArrestedAt,
                createdAtUtc:   a.CreatedAt,
                 createdBy:      a.CreatedBy,
                 arrestNum:      a.ArrestNum,
                 primaryIncidentId: a.PrimaryIncidentId);
            rm.ApplyDetailsChanged(a.NameId, a.ArrestedAt, a.ArrestTypeId, a.ArrestNum, a.PrimaryIncidentId);
            rm.ApplyLocationChanged(a.LocationId);
            rm.ApplyStatusChange(a.Status.ToString());
            rm.ApplyModifiedAudit(a.ModifiedBy, a.ModifiedAt, a.CreatedAt);
            return rm;
        }).ToList();

        _db.ArrestReadModels.AddRange(arrestReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        // ── 4. Rebuild Citations ─────────────────────────────────────────────
        var citations = await _db.Citations
            .AsNoTracking()
            .Where(c => c.JurisdictionId == jid)
            .ToListAsync(cancellationToken);

        var citationReadModels = citations.Select(c =>
        {
            var rm = CitationReadModel.Create(
                id:             c.Id,
                recordNumber:   c.RecordNumber,
                jurisdictionId: c.JurisdictionId,
                agencyId:       c.AgencyId,
                description:    c.Description,
                issueDate:      c.IssueDate,
                createdAtUtc:   c.CreatedAt,
                 createdBy:      c.CreatedBy,
                 citationNum:    c.CitationNum);
            rm.ApplyDetailsChanged(c.Description, c.IssueDate, c.CourtId, c.CitationNum);
            rm.ApplyLocationChanged(c.LocationId);
            rm.ApplyModifiedAudit(c.ModifiedBy, c.ModifiedAt, c.CreatedAt);
            if (c.IsIssued) rm.ApplyIssued();
            rm.ApplyModifiedAudit(c.ModifiedBy, c.ModifiedAt, c.CreatedAt);
            return rm;
        }).ToList();

        _db.CitationReadModels.AddRange(citationReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        // ── 5. Rebuild Incidents (include soft-deleted — they stay in read model as IsDeleted=true) ──
        var incidents = await _db.AllIncidentsWithDeleted
            .AsNoTracking()
            .Where(i => i.JurisdictionId == jid)
            .ToListAsync(cancellationToken);

        // Pre-compute arrest/citation link counts per incident
        var arrestCountByIncident = await _db.IncidentArrestLinks
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jid)
            .GroupBy(l => l.IncidentId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var citationCountByIncident = await _db.IncidentCitationLinks
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jid)
            .GroupBy(l => l.IncidentId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var incidentReadModels = incidents.Select(i =>
        {
            var rm = IncidentReadModel.Create(
                id:             i.Id,
                recordNumber:   i.RecordNumber,
                jurisdictionId: i.JurisdictionId,
                agencyId:       i.AgencyId,
                details:        new IncidentDetails
                {
                    IncidentNum = i.IncidentNum,
                    LocalNum    = i.LocalNum,
                    Description = i.Description,
                    CFSNum      = i.CFSNum,
                },
                status:         i.Status,
                 createdAtUtc:   i.CreatedAt,
                 createdBy:      i.CreatedBy);

            rm.ApplyLocationChanged(i.LocationId);
            rm.ApplyOccurredOnChanged(i.OccurredOn);
            if (i.IsDeleted) rm.ApplyDeleted();

            // Set denormalised counts directly via multiple increments would be slow;
            // use a helper method instead.
            var arrestCount  = arrestCountByIncident.GetValueOrDefault(i.Id);
            var citationCount = citationCountByIncident.GetValueOrDefault(i.Id);
            for (var x = 0; x < arrestCount;  x++) rm.IncrementArrestCount();
            for (var x = 0; x < citationCount; x++) rm.IncrementCitationCount();
            rm.ApplyModifiedAudit(i.ModifiedBy, i.ModifiedAt, i.CreatedAt);

            return rm;
        }).ToList();

        _db.IncidentReadModels.AddRange(incidentReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        // ── 6. Rebuild Arrest Links ──────────────────────────────────────────
        var arrestLinks = await _db.IncidentArrestLinks
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jid)
            .ToListAsync(cancellationToken);

        var incidentLookup = incidentReadModels.ToDictionary(r => r.Id);

        var arrestLinkReadModels = arrestLinks.Select(l =>
        {
            incidentLookup.TryGetValue(l.IncidentId, out var inc);
            return IncidentArrestLinkReadModel.Create(
                id:                    l.Id,
                jurisdictionId:        l.JurisdictionId,
                incidentId:            l.IncidentId,
                incidentRecordNumber:  inc?.RecordNumber ?? 0,
                incidentNum:           inc?.IncidentNum  ?? string.Empty,
                arrestId:              l.ArrestId,
                linkedAtUtc:           l.LinkedAtUtc);
        }).ToList();

        _db.IncidentArrestLinkReadModels.AddRange(arrestLinkReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        // ── 7. Rebuild Citation Links ────────────────────────────────────────
        var citationLinks = await _db.IncidentCitationLinks
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jid)
            .ToListAsync(cancellationToken);

        var citationLinkReadModels = citationLinks.Select(l =>
        {
            incidentLookup.TryGetValue(l.IncidentId, out var inc);
            return IncidentCitationLinkReadModel.Create(
                id:                    l.Id,
                jurisdictionId:        l.JurisdictionId,
                incidentId:            l.IncidentId,
                incidentRecordNumber:  inc?.RecordNumber ?? 0,
                incidentNum:           inc?.IncidentNum  ?? string.Empty,
                citationId:            l.CitationId,
                linkedAtUtc:           l.LinkedAtUtc);
        }).ToList();

        _db.IncidentCitationLinkReadModels.AddRange(citationLinkReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        // ── 8. Rebuild Mugshots ────────────────────────────────────────────────
        var mugshots = await _db.Mugshots
            .AsNoTracking()
            .Where(m => m.JurisdictionId == jid)
            .ToListAsync(cancellationToken);

        var mugshotReadModels = mugshots.Select(m => MugshotReadModel.Create(
            m.Id,
            m.JurisdictionId,
            m.AgencyId,
            m.FileName,
            m.ContentType,
            m.FileSizeBytes,
            m.StoragePath,
            m.PublicUrl,
            m.CapturedAtUtc,
            m.CreatedBy,
            m.CreatedAt)).ToList();

        _db.MugshotReadModels.AddRange(mugshotReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        // ── 9. Rebuild Mugshot Links and owner previews ───────────────────────
        var mugshotLinks = await _db.MugshotLinks
            .AsNoTracking()
            .Where(l => l.JurisdictionId == jid)
            .ToListAsync(cancellationToken);

        var mugshotLinkReadModels = mugshotLinks.Select(l => MugshotLinkReadModel.Create(
            l.Id,
            l.JurisdictionId,
            l.MugshotId,
            l.OwnerType,
            l.OwnerId,
            l.IsPrimary,
            l.DisplayOrder,
            l.LinkedAtUtc)).ToList();

        _db.MugshotLinkReadModels.AddRange(mugshotLinkReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        var mugshotUrlLookup = mugshotReadModels.ToDictionary(m => m.Id, m => m.PublicUrl);

        foreach (var group in mugshotLinkReadModels.GroupBy(l => new { l.OwnerType, l.OwnerId }))
        {
            var primaryLink = group
                .OrderByDescending(l => l.IsPrimary)
                .ThenBy(l => l.DisplayOrder)
                .ThenBy(l => l.LinkedAtUtc)
                .FirstOrDefault();

            var primaryUrl = primaryLink is not null
                ? mugshotUrlLookup.GetValueOrDefault(primaryLink.MugshotId)
                : null;

            if (group.Key.OwnerType == Domain.Common.MugshotOwnerTypes.Name)
            {
                var nameReadModel = nameReadModels.FirstOrDefault(n => n.Id == group.Key.OwnerId);
                nameReadModel?.ApplyPrimaryMugshot(primaryUrl);
            }
            else if (group.Key.OwnerType == Domain.Common.MugshotOwnerTypes.Arrest)
            {
                var arrestReadModel = arrestReadModels.FirstOrDefault(a => a.Id == group.Key.OwnerId);
                arrestReadModel?.ApplyPrimaryMugshot(primaryUrl);
            }
        }

        foreach (var nameReadModel in nameReadModels)
        {
            if (nameById.TryGetValue(nameReadModel.Id, out var name))
            {
                nameReadModel.ApplyModifiedAudit(name.ModifiedBy, name.ModifiedAt, name.CreatedAt);
            }
        }

        foreach (var arrestReadModel in arrestReadModels)
        {
            if (arrestById.TryGetValue(arrestReadModel.Id, out var arrest))
            {
                arrestReadModel.ApplyModifiedAudit(arrest.ModifiedBy, arrest.ModifiedAt, arrest.CreatedAt);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        sw.Stop();
        return new RebuildReadModelsResult(
            NamesRebuilt:          nameReadModels.Count,
            ArrestsRebuilt:        arrestReadModels.Count,
            CitationsRebuilt:      citationReadModels.Count,
            IncidentsRebuilt:      incidentReadModels.Count,
            ArrestLinksRebuilt:    arrestLinkReadModels.Count,
            CitationLinksRebuilt:  citationLinkReadModels.Count,
            MugshotsRebuilt:       mugshotReadModels.Count,
            MugshotLinksRebuilt:   mugshotLinkReadModels.Count,
            Elapsed:               sw.Elapsed);
    }
}

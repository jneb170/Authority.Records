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

        // ── 2. Rebuild Names (global filter excludes soft-deleted) ───────────
        var names = await _db.Names
            .AsNoTracking()
            .Where(n => n.JurisdictionId == jid)
            .ToListAsync(cancellationToken);

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
            rm.ApplyModifiedAudit(n.ModifiedBy);
            return rm;
        }).ToList();

        _db.NameReadModels.AddRange(nameReadModels);
        await _db.SaveChangesAsync(cancellationToken);

        // ── 3. Rebuild Arrests ───────────────────────────────────────────────
        var arrests = await _db.Arrests
            .AsNoTracking()
            .Where(a => a.JurisdictionId == jid)
            .ToListAsync(cancellationToken);

        var arrestReadModels = arrests.Select(a =>
        {
            var rm = ArrestReadModel.Create(
                id:             a.Id,
                recordNumber:   a.RecordNumber,
                jurisdictionId: a.JurisdictionId,
                agencyId:       a.AgencyId,
                suspectName:    a.SuspectName,
                arrestedAt:     a.ArrestedAt,
                createdAtUtc:   a.CreatedAt,
                createdBy:      a.CreatedBy,
                arrestNum:      a.ArrestNum);
            rm.ApplyDetailsChanged(a.SuspectName, a.ArrestedAt, a.ArrestTypeId, a.ArrestNum);
            rm.ApplyStatusChange(a.Status.ToString());
            rm.ApplyModifiedAudit(a.ModifiedBy);
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
            rm.ApplyModifiedAudit(c.ModifiedBy);
            if (c.IsIssued) rm.ApplyIssued();
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

            if (i.IsDeleted) rm.ApplyDeleted();
            rm.ApplyModifiedAudit(i.ModifiedBy);

            // Set denormalised counts directly via multiple increments would be slow;
            // use a helper method instead.
            var arrestCount  = arrestCountByIncident.GetValueOrDefault(i.Id);
            var citationCount = citationCountByIncident.GetValueOrDefault(i.Id);
            for (var x = 0; x < arrestCount;  x++) rm.IncrementArrestCount();
            for (var x = 0; x < citationCount; x++) rm.IncrementCitationCount();

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

        sw.Stop();
        return new RebuildReadModelsResult(
            NamesRebuilt:          nameReadModels.Count,
            ArrestsRebuilt:        arrestReadModels.Count,
            CitationsRebuilt:      citationReadModels.Count,
            IncidentsRebuilt:      incidentReadModels.Count,
            ArrestLinksRebuilt:    arrestLinkReadModels.Count,
            CitationLinksRebuilt:  citationLinkReadModels.Count,
            Elapsed:               sw.Elapsed);
    }
}

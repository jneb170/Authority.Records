using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Arrests.Queries;

internal static class ArrestDtoMapper
{
    public static async Task<IReadOnlyList<ArrestDto>> ToDtosAsync(
        IReadOnlyList<ArrestReadModel> arrests,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (arrests.Count == 0)
            return [];

        var nameIds = arrests
            .Where(a => a.NameId.HasValue)
            .Select(a => a.NameId!.Value)
            .Distinct()
            .ToList();

        var incidentIds = arrests
            .Where(a => a.PrimaryIncidentId.HasValue)
            .Select(a => a.PrimaryIncidentId!.Value)
            .Distinct()
            .ToList();

        var arrestIds = arrests
            .Select(a => a.Id)
            .Distinct()
            .ToList();

        var names = nameIds.Count > 0
            ? await dbContext.NameReadModels
                .AsNoTracking()
                .Where(n => nameIds.Contains(n.Id))
                .ToDictionaryAsync(n => n.Id, cancellationToken)
            : new Dictionary<Guid, NameReadModel>();

        var incidents = incidentIds.Count > 0
            ? await dbContext.IncidentReadModels
                .AsNoTracking()
                .Where(i => incidentIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, cancellationToken)
            : new Dictionary<Guid, IncidentReadModel>();

        var snapshots = arrestIds.Count > 0
            ? await dbContext.ArrestNameSnapshots
                .AsNoTracking()
                .Where(snapshot => arrestIds.Contains(snapshot.ArrestId))
                .ToDictionaryAsync(snapshot => snapshot.ArrestId, cancellationToken)
            : new Dictionary<Guid, ArrestNameSnapshot>();

        return arrests
            .Select(a =>
            {
                names.TryGetValue(a.NameId ?? Guid.Empty, out var name);
                incidents.TryGetValue(a.PrimaryIncidentId ?? Guid.Empty, out var incident);
                snapshots.TryGetValue(a.Id, out var snapshot);

                return a.ToDto(
                    suspectName: FormatName(name),
                    nameRecordNumber: name?.RecordNumber,
                    primaryIncidentRecordNumber: incident?.RecordNumber,
                    primaryIncidentNum: incident?.IncidentNum,
                    atTimeOfName: snapshot is null ? null : ArrestNameSnapshotBuilder.ToDto(snapshot));
            })
            .ToList();
    }

    private static string? FormatName(NameReadModel? name)
    {
        if (name is null)
            return null;

        if (name.NameType == NameTypes.Business)
            return name.LastOrBusinessName;

        if (string.IsNullOrWhiteSpace(name.FirstName))
            return name.LastOrBusinessName;

        return $"{name.LastOrBusinessName}, {name.FirstName}".Trim();
    }
}

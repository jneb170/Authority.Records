using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.ReadModels;
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

        var names = await dbContext.NameReadModels
            .AsNoTracking()
            .Where(n => nameIds.Contains(n.Id))
            .ToDictionaryAsync(n => n.Id, cancellationToken);

        var incidents = await dbContext.IncidentReadModels
            .AsNoTracking()
            .Where(i => incidentIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        return arrests
            .Select(a =>
            {
                names.TryGetValue(a.NameId ?? Guid.Empty, out var name);
                incidents.TryGetValue(a.PrimaryIncidentId ?? Guid.Empty, out var incident);

                return a.ToDto(
                    suspectName: FormatName(name),
                    nameRecordNumber: name?.RecordNumber,
                    primaryIncidentRecordNumber: incident?.RecordNumber,
                    primaryIncidentNum: incident?.IncidentNum);
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

using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;

namespace Modules.Records.Application.Admin.Queries.GetAuditLogs;

internal static class AuditLogQueryExecutor
{
    public static async Task<AuditLogQueryResult> ExecuteAsync(
        IApplicationDbContext db,
        IQueryable<AuditLogReadModel> query,
        AuditLogSearchRequest request,
        IUserLookupService userLookupService,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 10, 200);

        var availableSeverities = await query
            .Select(x => x.Severity)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var availableRecordTypes = await query
            .Select(x => x.RecordType)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var availableActionTypes = await query
            .Select(x => x.ActionType)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        query = await ApplyFiltersAsync(db, query, request, cancellationToken);
        query = ApplySorting(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var actorNames = await userLookupService.GetDisplayNamesAsync(
            rows.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value),
            cancellationToken);
        var recordLinks = await LoadRecordLinksAsync(db, rows, cancellationToken);

        var items = rows
            .Select(x => new AuditLogEntryDto(
                x.Id,
                x.OccurredOnUtc,
                x.Severity,
                x.RecordType,
                recordLinks.TryGetValue(x.AggregateId, out var link) ? link.RecordNumber : null,
                recordLinks.TryGetValue(x.AggregateId, out link) ? link.NavigationUrl : null,
                x.ActionType,
                x.EventType,
                x.JurisdictionId,
                x.AggregateId,
                x.AggregateVersion,
                x.UserId,
                ResolveActorDisplayName(x.UserId, actorNames),
                x.Message,
                x.Payload))
            .ToList();

        return new AuditLogQueryResult(
            items,
            totalCount,
            pageNumber,
            pageSize,
            availableSeverities,
            availableRecordTypes,
            availableActionTypes);
    }

    private static async Task<IQueryable<AuditLogReadModel>> ApplyFiltersAsync(
        IApplicationDbContext db,
        IQueryable<AuditLogReadModel> query,
        AuditLogSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Severity))
        {
            query = query.Where(x => x.Severity == request.Severity);
        }

        if (!string.IsNullOrWhiteSpace(request.RecordType))
        {
            query = query.Where(x => x.RecordType == request.RecordType);
        }

        if (request.RecordNumber.HasValue)
        {
            var aggregateIds = await FindAggregateIdsByRecordNumberAsync(db, request.RecordNumber.Value, cancellationToken);

            query = aggregateIds.Count == 0
                ? query.Where(_ => false)
                : query.Where(x => aggregateIds.Contains(x.AggregateId));
        }

        if (!string.IsNullOrWhiteSpace(request.ActionType))
        {
            query = query.Where(x => x.ActionType == request.ActionType);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId);
        }

        if (request.FromUtc.HasValue)
        {
            query = query.Where(x => x.OccurredOnUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            query = query.Where(x => x.OccurredOnUtc <= request.ToUtc.Value);
        }

        if (string.IsNullOrWhiteSpace(request.SearchText))
            return query;

        var searchText = request.SearchText.Trim();

        return query.Where(x =>
            x.Message.Contains(searchText) ||
            x.EventType.Contains(searchText) ||
            x.RecordType.Contains(searchText) ||
            x.ActionType.Contains(searchText) ||
            x.Payload.Contains(searchText));
    }

    private static async Task<HashSet<Guid>> FindAggregateIdsByRecordNumberAsync(
        IApplicationDbContext db,
        long recordNumber,
        CancellationToken cancellationToken)
    {
        var ids = new HashSet<Guid>();

        await AddAggregateIdsAsync(
            db.IncidentReadModels.Where(x => x.RecordNumber == recordNumber).Select(x => x.Id),
            ids,
            cancellationToken);

        await AddAggregateIdsAsync(
            db.ArrestReadModels.Where(x => x.RecordNumber == recordNumber).Select(x => x.Id),
            ids,
            cancellationToken);

        await AddAggregateIdsAsync(
            db.CitationReadModels.Where(x => x.RecordNumber == recordNumber).Select(x => x.Id),
            ids,
            cancellationToken);

        await AddAggregateIdsAsync(
            db.NameReadModels.Where(x => x.RecordNumber == recordNumber).Select(x => x.Id),
            ids,
            cancellationToken);

        await AddAggregateIdsAsync(
            db.LocationReadModels.Where(x => x.RecordNumber == recordNumber).Select(x => x.Id),
            ids,
            cancellationToken);

        return ids;
    }

    private static async Task AddAggregateIdsAsync(
        IQueryable<Guid> query,
        ISet<Guid> ids,
        CancellationToken cancellationToken)
    {
        foreach (var id in await query.ToListAsync(cancellationToken))
        {
            ids.Add(id);
        }
    }

    private static IQueryable<AuditLogReadModel> ApplySorting(
        IQueryable<AuditLogReadModel> query,
        AuditLogSearchRequest request)
    {
        return (request.SortField, request.SortDescending) switch
        {
            (AuditLogSortFields.Severity, false) => query.OrderBy(x => x.Severity).ThenByDescending(x => x.OccurredOnUtc),
            (AuditLogSortFields.Severity, true) => query.OrderByDescending(x => x.Severity).ThenByDescending(x => x.OccurredOnUtc),
            (AuditLogSortFields.RecordType, false) => query.OrderBy(x => x.RecordType).ThenByDescending(x => x.OccurredOnUtc),
            (AuditLogSortFields.RecordType, true) => query.OrderByDescending(x => x.RecordType).ThenByDescending(x => x.OccurredOnUtc),
            (AuditLogSortFields.ActionType, false) => query.OrderBy(x => x.ActionType).ThenByDescending(x => x.OccurredOnUtc),
            (AuditLogSortFields.ActionType, true) => query.OrderByDescending(x => x.ActionType).ThenByDescending(x => x.OccurredOnUtc),
            (AuditLogSortFields.EventType, false) => query.OrderBy(x => x.EventType).ThenByDescending(x => x.OccurredOnUtc),
            (AuditLogSortFields.EventType, true) => query.OrderByDescending(x => x.EventType).ThenByDescending(x => x.OccurredOnUtc),
            (AuditLogSortFields.OccurredOnUtc, false) => query.OrderBy(x => x.OccurredOnUtc),
            _ => query.OrderByDescending(x => x.OccurredOnUtc)
        };
    }

    private static string ResolveActorDisplayName(Guid? userId, IReadOnlyDictionary<Guid, string> actorNames)
    {
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return "System";

        return actorNames.TryGetValue(userId.Value, out var displayName)
            ? displayName
            : userId.Value.ToString();
    }

    private static async Task<Dictionary<Guid, (long RecordNumber, string NavigationUrl)>> LoadRecordLinksAsync(
        IApplicationDbContext db,
        IReadOnlyList<AuditLogReadModel> rows,
        CancellationToken cancellationToken)
    {
        var links = new Dictionary<Guid, (long RecordNumber, string NavigationUrl)>();

        await AddLinksAsync(
            rows, "Incident",
            ids => db.IncidentReadModels
                .Where(x => ids.Contains(x.Id))
                .Select(x => new RecordLinkProjection(x.Id, x.RecordNumber)),
            "/incidents/",
            links,
            cancellationToken);

        await AddLinksAsync(
            rows, "Arrest",
            ids => db.ArrestReadModels
                .Where(x => ids.Contains(x.Id))
                .Select(x => new RecordLinkProjection(x.Id, x.RecordNumber)),
            "/arrests/",
            links,
            cancellationToken);

        await AddLinksAsync(
            rows, "Citation",
            ids => db.CitationReadModels
                .Where(x => ids.Contains(x.Id))
                .Select(x => new RecordLinkProjection(x.Id, x.RecordNumber)),
            "/citations/",
            links,
            cancellationToken);

        await AddLinksAsync(
            rows, "Name",
            ids => db.NameReadModels
                .Where(x => ids.Contains(x.Id))
                .Select(x => new RecordLinkProjection(x.Id, x.RecordNumber)),
            "/names/",
            links,
            cancellationToken);

        await AddLinksAsync(
            rows, "Location",
            ids => db.LocationReadModels
                .Where(x => ids.Contains(x.Id))
                .Select(x => new RecordLinkProjection(x.Id, x.RecordNumber)),
            "/locations/",
            links,
            cancellationToken);

        return links;
    }

    private static async Task AddLinksAsync(
        IReadOnlyList<AuditLogReadModel> rows,
        string recordType,
        Func<List<Guid>, IQueryable<RecordLinkProjection>> queryFactory,
        string routePrefix,
        IDictionary<Guid, (long RecordNumber, string NavigationUrl)> links,
        CancellationToken cancellationToken)
    {
        var ids = rows
            .Where(x => x.RecordType == recordType)
            .Select(x => x.AggregateId)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return;

        var matches = await queryFactory(ids).ToListAsync(cancellationToken);
        foreach (var match in matches)
        {
            links[match.Id] = (match.RecordNumber, $"{routePrefix}{match.RecordNumber}");
        }
    }

    private sealed record RecordLinkProjection(Guid Id, long RecordNumber);
}

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Common.Exceptions;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Common.Behaviors;

/// <summary>
/// Abuse limits for the shared public "Try the demo" account. Real users are
/// never affected. Two limits, both configurable via <see cref="DemoRateLimitOptions"/>:
/// <list type="bullet">
/// <item>a per-write size cap (caps how much a single save can stuff into the
/// unbounded narrative/description fields); and</item>
/// <item>a rolling per-window cap on how many records the account may create.</item>
/// </list>
/// </summary>
public sealed class DemoRateLimitBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserContext _currentUser;
    private readonly ITenantProvider _tenantProvider;
    private readonly IApplicationDbContext _db;
    private readonly DemoRateLimitOptions _options;

    public DemoRateLimitBehavior(
        ICurrentUserContext currentUser,
        ITenantProvider tenantProvider,
        IApplicationDbContext db,
        IOptions<DemoRateLimitOptions> options)
    {
        _currentUser = currentUser;
        _tenantProvider = tenantProvider;
        _db = db;
        _options = options.Value;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsDemoUser)
            return await next();

        EnforceWriteSize(request);

        if (request is IRateLimitedCommand)
            await EnforceCreateRateAsync(cancellationToken);

        return await next();
    }

    private void EnforceWriteSize(TRequest request)
    {
        var limit = _options.MaxBytesPerWrite;
        if (limit <= 0)
            return;

        // Commands carrying legitimate large binary (e.g. image uploads) enforce
        // their own size limit and are exempt from this text-calibrated cap.
        if (request is IExemptFromDemoWriteSizeLimit)
            return;

        int bytes;
        try
        {
            bytes = JsonSerializer.SerializeToUtf8Bytes(request, request.GetType()).Length;
        }
        catch
        {
            // If a request can't be serialized we can't measure it; let it
            // through rather than block on a measurement failure. The create
            // rate cap still applies.
            return;
        }

        if (bytes > limit)
            throw new DemoLimitExceededException(
                $"This demo submission is too large ({bytes / 1024} KB). Demo accounts are limited to " +
                $"{limit / 1024} KB per save. Please shorten the entry and try again.");
    }

    private async Task EnforceCreateRateAsync(CancellationToken cancellationToken)
    {
        var limit = _options.MaxCreatesPerWindow;
        if (limit <= 0)
            return;

        var userId = _tenantProvider.GetUserId();
        var cutoff = DateTime.UtcNow.AddMinutes(-Math.Max(1, _options.WindowMinutes));

        // Count across every aggregate the demo account can create. IgnoreQueryFilters
        // so soft-deleted junk still counts — deleting doesn't reclaim storage.
        var created =
            await CountSince(_db.Incidents, userId, cutoff, cancellationToken)
            + await CountSince(_db.Arrests, userId, cutoff, cancellationToken)
            + await CountSince(_db.Citations, userId, cutoff, cancellationToken)
            + await CountSince(_db.Names, userId, cutoff, cancellationToken)
            + await CountSince(_db.Locations, userId, cutoff, cancellationToken)
            + await CountSince(_db.Mugshots, userId, cutoff, cancellationToken);

        if (created >= limit)
            throw new DemoLimitExceededException(
                $"Demo accounts are limited to {limit} new records per {_options.WindowMinutes} minutes. " +
                "Please wait a little while before creating more.");
    }

    private static Task<int> CountSince<T>(
        DbSet<T> set, Guid userId, DateTime cutoff, CancellationToken cancellationToken)
        where T : Domain.Common.Primitives.AggregateRoot
        => set.IgnoreQueryFilters()
              .CountAsync(e => e.CreatedBy == userId && e.CreatedAt >= cutoff, cancellationToken);
}

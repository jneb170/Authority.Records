using Infrastructure.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using System.Text.Json;

namespace Infrastructure.IntegrationTests.Outbox.FailurePaths;

/// <summary>
/// Regression tests for issue #93: closed generic domain events (lifecycle and lock events) were stored in the
/// outbox under their closed type name, which the registry never resolved, so every one was dead-lettered.
/// </summary>
public sealed class OutboxGenericEventTests : IntegrationTestBase
{
    public static TheoryData<IDomainEvent> GenericEvents => new()
    {
        new LifecycleStatusChangedDomainEvent<Incident>(Guid.NewGuid(), RecordStatus.Draft, RecordStatus.Open, Guid.NewGuid(), false),
        new LockAcquiredDomainEvent<Arrest>(Guid.NewGuid(), Guid.NewGuid()),
        new LockReleasedDomainEvent<Citation>(Guid.NewGuid(), Guid.NewGuid()),
    };

    [Theory]
    [MemberData(nameof(GenericEvents))]
    public async Task Outbox_ClosedGenericDomainEvent_IsProcessed_NotDeadLettered(IDomainEvent domainEvent)
    {
        var tenantId = Guid.NewGuid();
        Guid messageId;

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var message = new OutboxMessage(domainEvent, tenantId);
            messageId = message.Id;
            db.OutboxMessages.Add(message);
            await db.SaveChangesAsync();
        }

        using (var scope = ServiceProvider.CreateScope())
        {
            var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
            await processor.ProcessOutboxMessages(CancellationToken.None);
        }

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Empty(await db.DeadLetterMessages.ToListAsync());

            var processed = await db.OutboxMessages.IgnoreQueryFilters().SingleAsync(m => m.Id == messageId);
            Assert.NotNull(processed.ProcessedOnUtc);
            Assert.Null(processed.Error);
        }
    }

    [Theory]
    [MemberData(nameof(GenericEvents))]
    public void Registry_ResolvesClosedGenericType_AndPayloadRoundTripsAggregateId(IDomainEvent domainEvent)
    {
        var registry = ServiceProvider.GetRequiredService<DomainEventTypeRegistry>();
        var message = new OutboxMessage(domainEvent, Guid.NewGuid());

        Assert.True(registry.TryGet(message.Type, out var type));
        Assert.Equal(domainEvent.GetType(), type);

        var roundTripped = (IDomainEvent)JsonSerializer.Deserialize(message.Content, type!)!;
        Assert.Equal(domainEvent.AggregateId, roundTripped.AggregateId);
        Assert.Equal(domainEvent, roundTripped);
    }

    [Fact]
    public void Registry_RejectsClosedGenericTypes_ThatAreNotDomainEvents()
    {
        var registry = ServiceProvider.GetRequiredService<DomainEventTypeRegistry>();

        Assert.False(registry.TryGet(typeof(List<Incident>).AssemblyQualifiedName!, out var listType));
        Assert.Null(listType);
    }

    [Fact]
    public void Registry_RejectsGenericArguments_FromUntrustedAssemblies()
    {
        var registry = ServiceProvider.GetRequiredService<DomainEventTypeRegistry>();
        var name = typeof(LockAcquiredDomainEvent<Incident>).AssemblyQualifiedName!
            .Replace(typeof(Incident).Assembly.GetName().Name!, "Evil.Assembly");

        Assert.False(registry.TryGet(name, out var type));
        Assert.Null(type);
    }
}

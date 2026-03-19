using Infrastructure.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Outbox;
using Shared.Infrastructure.Persistence;
using System.Reflection;

namespace Infrastructure.IntegrationTests.Outbox.FailurePaths;

public sealed class OutboxFailurePathTests : IntegrationTestBase
{
    [Fact]
    public async Task Outbox_UnknownEventType_MovesMessageToDeadLetter()
    {
        var tenantId = Guid.NewGuid();

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var message = new OutboxMessage(new RetryBehavior.FailingDomainEvent(Guid.NewGuid()), tenantId);
            SetPrivateProperty(message, nameof(OutboxMessage.Type), "Missing.Event.Type, Missing.Assembly");

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
            Assert.Null(await db.OutboxMessages.FirstOrDefaultAsync());

            var deadLetter = await db.DeadLetterMessages.SingleAsync();
            Assert.Equal("Missing.Event.Type, Missing.Assembly", deadLetter.Type);
            Assert.Contains("Unknown type", deadLetter.LastError);
        }
    }

    [Fact]
    public async Task Outbox_InvalidPayload_MovesMessageToDeadLetter()
    {
        var tenantId = Guid.NewGuid();

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var message = new OutboxMessage(new RetryBehavior.FailingDomainEvent(Guid.NewGuid()), tenantId);
            SetPrivateProperty(message, nameof(OutboxMessage.Content), "{ this-is-not-valid-json");

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
            Assert.Null(await db.OutboxMessages.FirstOrDefaultAsync());

            var deadLetter = await db.DeadLetterMessages.SingleAsync();
            Assert.Contains("JsonException", deadLetter.LastError);
            Assert.Contains("FailingDomainEvent", deadLetter.Type);
        }
    }

    private static void SetPrivateProperty<TTarget, TValue>(TTarget target, string propertyName, TValue value)
    {
        var property = typeof(TTarget).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Property {propertyName} not found.");

        property.SetValue(target, value);
    }
}

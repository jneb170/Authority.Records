using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Outbox;

public sealed class DeadLetterQueueWriter
{
    public async Task DeadLetterAsync(
        OutboxMessage message,
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var deadLetterMessage = DeadLetterMessage.From(message);

        dbContext.DeadLetterMessages.Add(deadLetterMessage);
        dbContext.OutboxMessages.Remove(message);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RequeueAsync(
        Guid deadLetterId,
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var deadLetterMessage = await dbContext.DeadLetterMessages
            .FirstOrDefaultAsync(m => m.Id == deadLetterId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Dead letter message {deadLetterId} not found.");

        if (deadLetterMessage.IsRequeued)
            throw new InvalidOperationException(
                $"Dead letter message {deadLetterId} has already been requeued.");

        var requeued = OutboxMessage.FromDeadLetter(deadLetterMessage);

        deadLetterMessage.MarkRequeued();
        dbContext.OutboxMessages.Add(requeued);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Challenge.Credit.System.Shared.Outbox;

public interface IOutboxService
{
    void AddEvent<T>(T @event) where T : class;
}

public sealed class OutboxService<TDbContext>(
    ILogger<OutboxService<TDbContext>> logger,
    TDbContext context)
    : IOutboxService where TDbContext : DbContext
{
    public void AddEvent<T>(T @event) where T : class
    {
        ArgumentNullException.ThrowIfNull(@event);

        var outboxEvent = OutboxEvent.Create(@event);
        context.Set<OutboxEvent>().Add(outboxEvent);

        var entry = context.Entry(outboxEvent);
        if (entry.State == EntityState.Detached)
        {
            context.Set<OutboxEvent>().Attach(outboxEvent);
        }

        logger.LogDebug(
            "Outbox event added: {EventType} with ID {EventId}",
            @event,
            outboxEvent.Id);
    }
}
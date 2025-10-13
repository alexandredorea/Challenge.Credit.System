using Microsoft.EntityFrameworkCore;

namespace Challenge.Credit.System.Shared.Outbox;

public class OutboxService<TDbContext>(TDbContext context) : IOutboxService where TDbContext : DbContext
{
    public void AddEvent(string eventType, string payload)
    {
        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            CreatedAt = DateTime.UtcNow,
            Processed = false,
            RetryCount = 0
        };

        context.Set<OutboxEvent>().Add(outboxEvent);
    }
}
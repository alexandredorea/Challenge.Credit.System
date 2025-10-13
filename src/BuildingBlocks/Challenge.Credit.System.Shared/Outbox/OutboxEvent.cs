using System.Text.Json;

namespace Challenge.Credit.System.Shared.Outbox;

public sealed class OutboxEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public bool Processed { get; private set; } = false;
    public DateTime? ProcessedAt { get; private set; } = null;
    public int RetryCount { get; private set; } = 0;
    public string? ErrorMessage { get; private set; }

    private OutboxEvent()
    { }

    public static OutboxEvent Create<T>(T @event)
    {
        var eventType = @event!.GetType().Name;
        var payload = JsonSerializer.Serialize(@event);

        return new OutboxEvent
        {
            EventType = eventType,
            Payload = payload
        };
    }

    public void MarkProcessed()
    {
        Processed = true;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        RetryCount++;
        ErrorMessage = error;
        ProcessedAt = null;
    }
}
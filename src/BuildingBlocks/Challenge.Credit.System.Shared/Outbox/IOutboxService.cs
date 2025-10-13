namespace Challenge.Credit.System.Shared.Outbox;

public interface IOutboxService
{
    void AddEvent(string eventType, string payload);
}
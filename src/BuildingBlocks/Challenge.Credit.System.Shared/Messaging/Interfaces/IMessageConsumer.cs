namespace Challenge.Credit.System.Shared.Messaging.Interfaces;

public interface IMessageConsumer
{
    Task ConsumeAsync(string message, CancellationToken cancellationToken = default);
}

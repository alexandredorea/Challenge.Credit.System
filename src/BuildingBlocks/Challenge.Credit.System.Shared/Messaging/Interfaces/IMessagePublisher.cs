namespace Challenge.Credit.System.Shared.Messaging.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);

    Task PublishAsync(string queueName, string payload, CancellationToken cancellationToken = default);
}
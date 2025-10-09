using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace Challenge.Credit.System.Shared.Messaging;

internal sealed class RabbitMqPublisher(
    string hostName,
    ILogger<RabbitMqPublisher> logger,
    string exchangeName = "credit-system") : IMessagePublisher, IDisposable
{
    public Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        //TODO:
        GC.SuppressFinalize(this);
    }
}

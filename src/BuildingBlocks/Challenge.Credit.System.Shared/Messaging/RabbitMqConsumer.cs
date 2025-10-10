namespace Challenge.Credit.System.Shared.Messaging;

using Microsoft.Extensions.Hosting;

internal class RabbitMqConsumer : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}

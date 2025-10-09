using Microsoft.Extensions.Hosting;

namespace Challenge.Credit.System.Shared.Messaging
{
    internal class RabbitMqConsumer : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}

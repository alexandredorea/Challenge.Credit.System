using Challenge.Credit.System.Shared.Messaging;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjections
{
    public static IHostApplicationBuilder AddRabbitMqService(this IHostApplicationBuilder builder)
    {
        // Obtem o hostName do RabbitMQ da configuração
        var hostName = builder.Configuration["RabbitMq:HostName"];
        if (string.IsNullOrWhiteSpace(hostName))
            throw new ArgumentException("RabbitMQ: 'Hostname' do servidor não encontrada.");

        builder.Services.AddSingleton<IMessagePublisher>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqPublisher>>();
            return new RabbitMqPublisher(hostName, logger);
        });

        // Registrar o serviço de inicialização da mensageria
        builder.Services.AddHostedService<AsyncInitializationService>();

        // Registrar a classe como IAsyncInitializable
        builder.Services.AddSingleton<IAsyncInitializable>(sp => sp.GetRequiredService<IMessagePublisher>() as IAsyncInitializable);

        return builder;
    }
}
namespace Microsoft.Extensions.DependencyInjection;

using Challenge.Credit.System.Shared.Messaging;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

public static class DependencyInjections
{
    public static IServiceCollection AddRabbitMqService(this IServiceCollection services, IConfiguration configuration)
    {
        //TODO: adicionar as dependencias


        // Pega RabbitMQ hostname da configuração
        var rabbitMqHost = configuration["RabbitMq:HostName"];
        if (string.IsNullOrWhiteSpace(rabbitMqHost))
            throw new ArgumentException("");
        
        // Registra RabbitMQ Publisher as a singleton
        services.AddSingleton<IMessagePublisher>(sp => new RabbitMqPublisher(rabbitMqHost, new NullLogger<RabbitMqPublisher>()));

        return services;
    }


}

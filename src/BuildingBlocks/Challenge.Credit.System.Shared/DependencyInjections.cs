using Challenge.Credit.System.Shared.Events.Clients;
using Challenge.Credit.System.Shared.Events.CreditProposals;
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
            var publisher = new RabbitMqPublisher(logger, hostName);

            // TODO: refazer o registro dos bindings conhecidos para ser dinamico para atender os consumers registrados
            publisher.RegisterBinding(nameof(ClientCreatedEvent), "credit-system", nameof(ClientCreatedEvent));
            publisher.RegisterBinding(nameof(CreditProposalApprovedEvent), "credit-system", nameof(CreditProposalApprovedEvent));
            publisher.RegisterBinding(nameof(CreditProposalRejectedEvent), "credit-system", nameof(CreditProposalRejectedEvent));

            return publisher;
        });

        // Registrar o serviço de inicialização da mensageria
        builder.Services.AddHostedService<AsyncInitializationService>();

        // Registrar a classe como IAsyncInitializable
        builder.Services.AddSingleton<IAsyncInitializable>(sp => sp.GetRequiredService<IMessagePublisher>() as IAsyncInitializable);

        return builder;
    }
}
using Challenge.Credit.System.Module.CreditCard.Consumers;
using Challenge.Credit.System.Module.CreditCard.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditCard.Core.Application.Services;
using Challenge.Credit.System.Module.CreditCard.Infrastructure.Data;
using Challenge.Credit.System.Shared.Events.CreditProposals;
using Challenge.Credit.System.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjections
{
    public static IHostApplicationBuilder AddCreditCardModule(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase();
        builder.AddServices();
        builder.AddConsumers();

        return builder;
    }

    private static void AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<CardDbContext>(options => options.UseInMemoryDatabase($"CardDb"));
        builder.Services.AddScoped<ICardDbContext>(provider => provider.GetRequiredService<CardDbContext>());
    }

    private static void AddServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ICardGeneratorService, CardGeneratorService>();
        builder.Services.AddScoped<ICardService, CardService>();
    }

    private static void AddConsumers(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<CreditProposalApprovedEventConsumer>();

        var hostName = builder.Configuration["RabbitMq:HostName"] ?? "localhost";
        var exchangeName = builder.Configuration["RabbitMq:ExchangeName"] ?? "credit-system";
        var queueName = builder.Configuration["RabbitMq:QueueName"] ?? nameof(CreditProposalApprovedEvent);
        var routingKey = builder.Configuration["RabbitMq:RoutingKey"] ?? nameof(CreditProposalApprovedEvent);
        builder.Services.AddSingleton<IHostedService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqConsumer>>();
            var consumer = new RabbitMqConsumer(
                serviceProvider: sp,
                logger: sp.GetRequiredService<ILogger<RabbitMqConsumer>>(),
                hostName: hostName,
                queueName: queueName,
                exchangeName: exchangeName,
                routingKey: routingKey,
                messageHandlerType: typeof(CreditProposalApprovedEventConsumer));

            logger.LogInformation("Registrando consumer para fila: {QueueName}", queueName);
            return consumer;
        });
    }
}
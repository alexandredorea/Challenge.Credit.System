using Challenge.Credit.System.Module.CreditCard.Consumers;
using Challenge.Credit.System.Module.CreditCard.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditCard.Core.Application.Services;
using Challenge.Credit.System.Module.CreditCard.Infrastructure.Data;
using Challenge.Credit.System.Shared.Messaging;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
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
        builder.Services.AddDbContext<CardDbContext>(options => options.UseInMemoryDatabase("CartaoDb"));
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
        builder.Services.AddScoped<IMessageConsumer>(sp => sp.GetRequiredService<CreditProposalApprovedEventConsumer>());

        builder.Services.AddHostedService(sp => new RabbitMqConsumer(
            serviceProvider: sp,
            logger: sp.GetRequiredService<ILogger<RabbitMqConsumer>>(),
            hostName: builder.Configuration["RabbitMq:HostName"] ?? "localhost",
            queueName: builder.Configuration["RabbitMq:QueueName"] ?? "proposta.aprovada",
            exchangeName: builder.Configuration["RabbitMq:ExchangeName"] ?? "credit-system",
            routingKey: builder.Configuration["RabbitMq:RoutingKey"] ?? "proposta.aprovada",
            messageHandlerType: typeof(CreditProposalApprovedEventConsumer)
        ));
    }
}
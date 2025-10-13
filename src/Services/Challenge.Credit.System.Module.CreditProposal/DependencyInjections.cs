using Challenge.Credit.System.Module.CreditProposal.Consumers;
using Challenge.Credit.System.Module.CreditProposal.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Application.Services;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;
using Challenge.Credit.System.Module.CreditProposal.Infrastructure.Data;
using Challenge.Credit.System.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjections
{
    public static IHostApplicationBuilder AddCreditProposalModule(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase();
        builder.AddServices();
        builder.AddConsumers();

        return builder;
    }

    private static void AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ProposalDbContext>(options => options.UseInMemoryDatabase("ProposalDb"));
        builder.Services.AddScoped<IProposalDbContext>(provider => provider.GetRequiredService<ProposalDbContext>());
    }

    private static void AddServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProposalService, ProposalService>();

        // Registrar políticas de score
        builder.Services.AddScoped<IScoreCalculator, ScoreCalculator>();
        builder.Services.AddScoped<IScoreEvaluator, ScoreEvaluator>();
        builder.Services.AddScoped<IScorePolicy, LowScorePolicy>();
        builder.Services.AddScoped<IScorePolicy, MediumScorePolicy>();
        builder.Services.AddScoped<IScorePolicy, HighScorePolicy>();
    }

    private static void AddConsumers(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ClientCreatedEventConsumer>();

        // Registra o RabbitMqConsumer como HostedService (Worker)
        var hostName = builder.Configuration["RabbitMq:HostName"] ?? "localhost";
        var exchangeName = builder.Configuration["RabbitMq:ExchangeName"] ?? "credit-system";  // TODO: ajustar o arquivo de configuracao o mesmo exchange do publisher
        var queueName = builder.Configuration["RabbitMq:QueueName"] ?? "cliente.cadastrado";   // TODO: ajustar o arquivo de configuracao o nome da fila específica
        var routingKey = builder.Configuration["RabbitMq:RoutingKey"] ?? "cliente.cadastrado"; // TODO: ajustar o arquivo de configuracao a mesma routing key usada no publish

        // Registrar o RabbitMqConsumer como HostedService (Worker) para consumir a fila 'cliente.cadastrado'
        builder.Services.AddSingleton<IHostedService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqConsumer>>();
            var consumer = new RabbitMqConsumer(
                serviceProvider: sp,
                logger: logger,
                hostName: hostName,
                queueName: queueName,
                exchangeName: exchangeName,
                routingKey: routingKey,
                messageHandlerType: typeof(ClientCreatedEventConsumer));

            logger.LogInformation("Registrando consumer para fila: {QueueName}", queueName);
            return consumer;
        });
    }
}
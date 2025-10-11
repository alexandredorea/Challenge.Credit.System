using Challenge.Credit.System.Module.CreditProposal.Consumers;
using Challenge.Credit.System.Module.CreditProposal.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Application.Services;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;
using Challenge.Credit.System.Module.CreditProposal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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
        //builder.Services.AddScoped<IMessageConsumer, ClientCreatedEventConsumer>();
        builder.Services.AddScoped<ClientCreatedEventConsumer>();
    }
}
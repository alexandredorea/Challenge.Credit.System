using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Interfaces;
using Challenge.Credit.System.Module.CreditProposal.Core.Domain.Services;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjections
{
    public static IHostApplicationBuilder AddCreditProposalModule(this IHostApplicationBuilder builder)
    {
        //TODO: adicionar as dependencias

        // Registrar políticas de score
        builder.Services.AddScoped<IScoreCalculator, ScoreCalculator>();
        builder.Services.AddScoped<IScoreEvaluator, ScoreEvaluator>();
        builder.Services.AddScoped<IScorePolicy, LowScorePolicy>();
        builder.Services.AddScoped<IScorePolicy, MediumScorePolicy>();
        builder.Services.AddScoped<IScorePolicy, HighScorePolicy>();

        return builder;
    }
}
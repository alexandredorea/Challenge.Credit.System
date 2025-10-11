using Challenge.Credit.System.Module.CreditCard.Consumers;
using Challenge.Credit.System.Module.CreditCard.Core.Application.Interfaces;
using Challenge.Credit.System.Module.CreditCard.Core.Application.Services;
using Challenge.Credit.System.Module.CreditCard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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
    }
}
using Challenge.Credit.System.Module.Client.Core.Application.Interfaces;
using Challenge.Credit.System.Module.Client.Core.Application.Services;
using Challenge.Credit.System.Module.Client.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjections
{
    public static IHostApplicationBuilder AddClientModule(this IHostApplicationBuilder builder)
    {
        builder.AddDatabase();
        builder.AddServices();

        return builder;
    }

    private static void AddDatabase(this IHostApplicationBuilder builder)
    {
        //var connectionString = builder.Configuration.GetConnectionString("ClientDb");
        //if (string.IsNullOrWhiteSpace(connectionString))
        //    throw new ArgumentException("ConnectionString 'ClientDb' não encontrada.");

        builder.Services.AddDbContext<ClientDbContext>(options => options.UseInMemoryDatabase("ClientDb"));
        builder.Services.AddScoped<IClientDbContext>(provider => provider.GetRequiredService<ClientDbContext>());
    }

    private static void AddServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IClientService, ClientService>();
    }
}
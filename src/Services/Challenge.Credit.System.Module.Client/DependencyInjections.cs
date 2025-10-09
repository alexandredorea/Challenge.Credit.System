using Challenge.Credit.System.Module.Client.Core.Application.Interfaces;
using Challenge.Credit.System.Module.Client.Core.Application.Services;
using Challenge.Credit.System.Module.Client.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjections
{
    public static IHostApplicationBuilder AddClientModule(this IHostApplicationBuilder builder)
    {

        //var connectionString = builder.Configuration.GetConnectionString("ClientDb");
        //if (string.IsNullOrWhiteSpace(connectionString)) 
        //    throw new ArgumentException("Connection string 'ClientDb' not found.");

        builder.Services.AddDbContext<ClientDbContext>(options => options.UseInMemoryDatabase("ClientDb"));
        builder.Services.AddScoped<IClientDbContext>(provider => provider.GetRequiredService<ClientDbContext>());

        builder.Services.AddScoped<IClientService, ClientService>();
        return builder;
    }
}

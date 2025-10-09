using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Challenge.Credit.System.Shared.Messaging;

public sealed class AsyncInitializationService(IEnumerable<IAsyncInitializable> initializables) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var initializable in initializables)
        {
            await initializable.InitializeAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

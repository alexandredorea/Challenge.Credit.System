namespace Challenge.Credit.System.Shared.Messaging.Interfaces;

/// <summary>
/// Interface para garantir a inicialização assíncrona da mensageria
/// </summary>
public interface IAsyncInitializable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using RabbitMQ.Client.Exceptions;

namespace Challenge.Credit.System.Shared.Policies;

public static class ResiliencePolicies
{
    /// <summary>
    /// Política de retry com backoff exponencial.
    /// </summary>
    public static AsyncRetryPolicy CreateRetryPolicy(ILogger logger, int maxRetryAttempts = 5)
    {
        return Policy
            .Handle<BrokerUnreachableException>()
            .Or<Exception>() // Ou outra, sendo mais específico
            .WaitAndRetryAsync(
                retryCount: maxRetryAttempts,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Tentativa {RetryCount} de {MaxRetries}. Aguardando {Delay}s. Erro: {Message}",
                        retryCount,
                        maxRetryAttempts,
                        timeSpan.TotalSeconds,
                        exception.Message);
                });
    }

    /// <summary>
    /// Política de retry específica para operações de mensageria
    /// </summary>
    public static AsyncRetryPolicy CreateMessagingRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Falha ao processar mensagem. Tentativa {RetryCount}. Aguardando {Delay}s antes de tentar novamente.",
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    /// <summary>
    /// Política de circuit breaker para evitar cascata de falhas
    /// </summary>
    public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(
                        exception,
                        "Circuit breaker aberto por {Duration}s devido a falhas consecutivas",
                        duration.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker resetado");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker em estado half-open, testando recuperação");
                });
    }

    /// <summary>
    /// Política de timeout para evitar operações que demoram muito
    /// </summary>
    public static AsyncTimeoutPolicy CreateTimeoutPolicy(int timeoutSeconds = 30)
    {
        return Policy.TimeoutAsync(TimeSpan.FromSeconds(timeoutSeconds));
    }
}

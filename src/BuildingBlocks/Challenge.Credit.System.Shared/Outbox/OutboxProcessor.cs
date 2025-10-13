using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Challenge.Credit.System.Shared.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Retry;

namespace Challenge.Credit.System.Shared.Outbox;

public class OutboxProcessor<TDbContext> : BackgroundService where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor<TDbContext>> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private const int MaxRetryAttempts = 5;
    private const int ProcessingIntervalSeconds = 5;
    private const int BatchSize = 100;

    public OutboxProcessor(
        ILogger<OutboxProcessor<TDbContext>> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        _retryPolicy = ResiliencePolicies.CreateRetryPolicy(_logger, maxRetryAttempts: 3);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor iniciado para {DbContext}", typeof(TDbContext).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar eventos da Outbox");
            }

            await Task.Delay(TimeSpan.FromSeconds(ProcessingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor finalizado");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        // Busca eventos pendentes
        var outboxEvents = await context.Set<OutboxEvent>()
            .Where(e => !e.Processed && e.RetryCount < MaxRetryAttempts)
            .OrderBy(e => e.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (outboxEvents.Count == 0)
            return;

        _logger.LogInformation("Processando {Count} eventos pendentes", outboxEvents.Count);

        foreach (var outboxEvent in outboxEvents)
        {
            try
            {
                // Tenta publicar no RabbitMQ com retry
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await messagePublisher.PublishAsync(
                        outboxEvent.EventType,
                        outboxEvent.Payload,
                        cancellationToken);
                });

                // Marca como processado
                outboxEvent.Processed = true;
                outboxEvent.ProcessedAt = DateTime.UtcNow;
                outboxEvent.ErrorMessage = null;

                _logger.LogInformation(
                    "Evento {EventId} do tipo {EventType} publicado com sucesso",
                    outboxEvent.Id, outboxEvent.EventType);
            }
            catch (Exception ex)
            {
                // Incrementa contador de tentativas
                outboxEvent.RetryCount++;
                outboxEvent.ErrorMessage = ex.Message;

                _logger.LogError(ex,
                    "Falha ao publicar evento {EventId} do tipo {EventType}. Tentativa {RetryCount}/{MaxRetry}",
                    outboxEvent.Id, outboxEvent.EventType, outboxEvent.RetryCount, MaxRetryAttempts);

                if (outboxEvent.RetryCount >= MaxRetryAttempts)
                {
                    _logger.LogError(
                        "Evento {EventId} atingiu o número máximo de tentativas e será marcado como falho",
                        outboxEvent.Id);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Challenge.Credit.System.Shared.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Retry;

namespace Challenge.Credit.System.Shared.Outbox;

public sealed class OutboxProcessor<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor<TDbContext>> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    private const int MaxRetryAttempts = 5;
    private const int ProcessingIntervalSeconds = 5;
    private const int BatchSize = 100;
    private const int LockTimeoutSeconds = 30;

    public OutboxProcessor(
        ILogger<OutboxProcessor<TDbContext>> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _retryPolicy = ResiliencePolicies.CreateRetryPolicy(_logger);
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
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OutboxProcessor cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro crítico ao processar eventos da Outbox");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(ProcessingIntervalSeconds),
                stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor finalizado");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        //var cutoffTime = DateTime.UtcNow.AddSeconds(-LockTimeoutSeconds);

        var pendingEvent = await context.Set<OutboxEvent>()
            //.Where(e => !e.Processed && e.RetryCount < MaxRetryAttempts)
            //.Where(e => e.ProcessedAt == null || e.ProcessedAt < cutoffTime)
            .OrderBy(e => e.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pendingEvent.Count == 0)
            return;

        _logger.LogInformation(
            "Processando {Count} eventos pendentes da Outbox",
            pendingEvent.Count);

        foreach (var @event in pendingEvent)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            await ProcessSingleEventAsync(publisher, @event, cancellationToken);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao salvar mudanças no contexto após processar eventos");
        }
    }

    private async Task ProcessSingleEventAsync(IMessagePublisher publisher, OutboxEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await publisher.PublishAsync(
                    @event.EventType,
                    @event.Payload,
                    cancellationToken);
            });

            @event.MarkProcessed();
            _logger.LogInformation("Evento {EventId} publicado com sucesso ({EventType})",
                @event.Id,
                @event.EventType);
        }
        catch (Exception ex)
        {
            @event.MarkFailed(TruncateErrorMessage(ex.Message));
            _logger.LogError(
                ex,
                "Falha ao publicar evento {EventId} do tipo {EventType}. " +
                "Tentativa {RetryCount}/{MaxRetry}",
                @event.Id,
                @event.EventType,
                @event.RetryCount,
                MaxRetryAttempts);

            if (@event.RetryCount >= MaxRetryAttempts)
                await SendAlertAsync(@event, ex);
        }
    }

    private static string TruncateErrorMessage(string message, int maxLength = 2000)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        return message.Length <= maxLength
            ? message
            : message[..maxLength];
    }

    private Task SendAlertAsync(OutboxEvent outboxEvent, Exception ex)
    {
        // TODO: Implementar notificação (email, Slack, Teams, etc)
        _logger.LogCritical(
            "ALERTA: Intervenção manual necessária. " +
            "Evento {EventId} falhou permanentemente após {RetryCount} tentativas. " +
            "Tipo: {EventType}, Erro: {Error}",
            outboxEvent.Id,
            outboxEvent.RetryCount,
            outboxEvent.EventType,
            ex.Message);

        return Task.CompletedTask;
    }
}
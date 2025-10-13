using System.Text;
using System.Text.Json;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Challenge.Credit.System.Shared.Policies;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using RabbitMQ.Client;

namespace Challenge.Credit.System.Shared.Messaging;

public sealed class RabbitMqPublisher : IMessagePublisher, IAsyncInitializable, IAsyncDisposable
{
    private readonly string _exchangeName;
    private readonly string _hostName;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IChannel? _channel;
    private IConnection? _connection;
    private readonly AsyncRetryPolicy _connectionRetryPolicy;
    private readonly AsyncRetryPolicy _publishRetryPolicy;

    private readonly List<(string queueName, string exchangeName, string routingKey)> _knownBindings = [];

    public RabbitMqPublisher(
        ILogger<RabbitMqPublisher> logger,
        string hostName,
        string exchangeName = "credit-system")
    {
        _hostName = hostName;
        _logger = logger;
        _exchangeName = exchangeName;

        // Política de retry para a conexão inicial
        _connectionRetryPolicy = ResiliencePolicies.CreateRetryPolicy(_logger, maxRetryAttempts: 3);

        // Política de retry para a publicação de mensagens
        _publishRetryPolicy = ResiliencePolicies.CreateMessagingRetryPolicy(_logger);
    }

    public void RegisterBinding(string queueName, string exchangeName, string routingKey)
    {
        _knownBindings.Add((queueName, exchangeName, routingKey));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_connection != null && _connection.IsOpen)
            return;

        await _connectionRetryPolicy.ExecuteAsync(async (_cancellationToken) =>
        {
            var factory = new ConnectionFactory
            {
                //Uri = new Uri("ConnectionString"),
                HostName = _hostName,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync(_cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: _cancellationToken);

            await SetupTopologyAsync(_channel, cancellationToken);

            _logger.LogInformation("Publisher resiliente criado para exchange: {ExchangeName}", _exchangeName);
        }, cancellationToken);
    }

    private async Task SetupTopologyAsync(IChannel channel, CancellationToken stoppingToken)
    {
        foreach (var (queueName, exchangeName, routingKey) in _knownBindings)
        {
            // Nomes para DLX e DLQ
            var dlxName = $"{exchangeName}.dlx";
            var dlqName = $"{queueName}.dlq";

            // Declarar Dead Letter Exchange
            await channel.ExchangeDeclareAsync(exchange: dlxName, type: ExchangeType.Direct, durable: true, cancellationToken: stoppingToken);
            // Declarar Dead Letter Queue
            await channel.QueueDeclareAsync(queue: dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            // Bind DLQ ao DLX
            await channel.QueueBindAsync(queue: dlqName, exchange: dlxName, routingKey: dlqName, cancellationToken: stoppingToken);
            // Declarar exchange principal
            await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

            // Declarar fila principal com DLX configurado
            var arguments = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", dlxName },
                { "x-dead-letter-routing-key", dlqName }
            };
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments, cancellationToken: stoppingToken);

            // Bind da fila principal à exchange principal
            await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey, cancellationToken: stoppingToken);

            _logger.LogDebug("Fila '{QueueName}' e binding para exchange '{ExchangeName}' com routing key '{RoutingKey}' configurados.", queueName, exchangeName, routingKey);
        }
    }

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        // Boa prática: criar um canal por publicação ou reutilizar um de um pool.
        if (_channel == null || !_channel.IsOpen)
            throw new InvalidOperationException("O publisher não foi inicializado. Chame InitializeAsync primeiro.");

        var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await _publishRetryPolicy.ExecuteAsync(async (ct) =>
        {
            try
            {
                await _channel.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    mandatory: true, // 'mandatory: true' ajuda a detectar quando a msg não vai para nenhuma fila
                    body: messageBody,
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar mensagem para routing key: {RoutingKey}", routingKey);
                throw;
            }
        }, cancellationToken);
    }

    public async Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken = default)
    {
        if (_channel == null || !_channel.IsOpen)
            throw new InvalidOperationException("O publisher não foi inicializado. Chame InitializeAsync primeiro.");

        if (string.IsNullOrWhiteSpace(routingKey))
            throw new ArgumentException("Routing key cannot be null or empty", nameof(routingKey));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload cannot be null or empty", nameof(payload));

        var messageBody = Encoding.UTF8.GetBytes(payload);

        await _publishRetryPolicy.ExecuteAsync(async (ct) =>
        {
            try
            {
                await _channel.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    body: messageBody,
                    cancellationToken: ct);

                _logger.LogDebug(
                    "Mensagem publicada com sucesso para routing key: {RoutingKey}",
                    routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao publicar mensagem para routing key: {RoutingKey}",
                    routingKey);
                throw;
            }
        }, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            try
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fechar o canal RabbitMQ durante DisposeAsync.");
            }
            finally
            {
                _channel = null;
            }
        }

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fechar a conexão RabbitMQ durante DisposeAsync.");
            }
            finally
            {
                _connection = null;
            }
        }

        GC.SuppressFinalize(this);
    }
}
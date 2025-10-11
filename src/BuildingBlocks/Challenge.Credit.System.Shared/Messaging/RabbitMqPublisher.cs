using System.Text;
using System.Text.Json;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Challenge.Credit.System.Shared.Policies;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using RabbitMQ.Client;

namespace Challenge.Credit.System.Shared.Messaging;

internal sealed class RabbitMqPublisher : IMessagePublisher, IAsyncInitializable, IAsyncDisposable
{
    private readonly AsyncRetryPolicy _connectionRetryPolicy;
    private readonly string _exchangeName;
    private readonly string _hostName;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly AsyncRetryPolicy _publishRetryPolicy;
    private IChannel? _channel;
    private IConnection? _connection;

    public RabbitMqPublisher(
        string hostName,
        ILogger<RabbitMqPublisher> logger,
        string exchangeName = "credit-system")
    {
        _hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exchangeName = exchangeName;

        // Política de retry para a conexão inicial
        _connectionRetryPolicy = ResiliencePolicies.CreateRetryPolicy(_logger, maxRetryAttempts: 3);

        // Política de retry para a publicação de mensagens
        _publishRetryPolicy = ResiliencePolicies.CreateMessagingRetryPolicy(_logger);
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

            await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: _cancellationToken);

            _logger.LogInformation("Publisher resiliente criado para exchange: {ExchangeName}", _exchangeName);
        }, cancellationToken);
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
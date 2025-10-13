using System.Text;
using Challenge.Credit.System.Shared.Messaging.Interfaces;
using Challenge.Credit.System.Shared.Policies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Challenge.Credit.System.Shared.Messaging;

public sealed class RabbitMqConsumer : BackgroundService, IAsyncDisposable
{
    private IChannel? _channel;
    private IConnection? _connection;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _hostName;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKey;
    private readonly Type _messageHandlerType;
    private readonly List<(string queueName, string exchangeName, string routingKey)> _knownBindings = [];

    public RabbitMqConsumer(
        ILogger<RabbitMqConsumer> logger,
        IServiceProvider serviceProvider,
        string hostName,
        string queueName,
        string exchangeName,
        string routingKey,
        Type messageHandlerType)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hostName = hostName;
        _queueName = queueName;
        _exchangeName = exchangeName;
        _routingKey = routingKey;
        _messageHandlerType = messageHandlerType;
        _knownBindings.Add((queueName, exchangeName, routingKey));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando consumer resiliente para fila: {QueueName}", _queueName);
        stoppingToken.Register(() => _logger.LogInformation("Cancelamento solicitado para fila '{QueueName}'. Fechando conexão do consumer...", _queueName));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndSetupAsync(stoppingToken);
                _logger.LogInformation("Consumer conectado e escutando a fila '{QueueName}'.", _queueName);

                // Mantém o método em execução aguardando até que o token de cancelamento seja acionado.
                // A lógica de consumo acontece nos eventos do canal.
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Exceção esperada quando a aplicação está parando. Não é um erro.
                //_logger.LogInformation(ex, "Processamento da mensagem cancelado devido ao desligamento do serviço. A mensagem retornará à fila '{QueueName}'.", _queueName);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no Consumer da fila '{QueueName}'. Tentando reconectar em 5 segundos.", _queueName);
                await CleanupConnection(); // Limpa os recursos antes de tentar reconectar
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("O consumer para a fila '{QueueName}' foi finalizado.", _queueName);
    }

    private async Task ConnectAndSetupAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        };

        _connection = await Task.Run(() => factory.CreateConnectionAsync(), stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Configurar a topologia (exchanges, filas, bindings) de forma assíncrona
        await SetupTopologyAsync(_channel, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceived;

        // Configurar QoS (Quality of Service)
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, stoppingToken);
        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer, stoppingToken);
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var messageBody = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Mensagem recebida na fila {QueueName}. DeliveryTag: {DeliveryTag}", _queueName, ea.DeliveryTag);

            var messageRetryPolicy = ResiliencePolicies.CreateMessagingRetryPolicy(_logger);

            await messageRetryPolicy.ExecuteAsync(async () =>
            {
                // Cria um escopo de injeção de dependência para cada mensagem
                using var scope = _serviceProvider.CreateScope();
                var messageHandler = scope.ServiceProvider.GetRequiredService(_messageHandlerType) as IMessageConsumer;

                if (messageHandler is null)
                {
                    _logger.LogCritical("Não foi possível resolver o handler do tipo '{HandlerType}'", _messageHandlerType.Name);
                    throw new InvalidOperationException($"Não foi possível resolver o handler do tipo '{_messageHandlerType.Name}'");
                }

                await messageHandler.ConsumeAsync(messageBody, CancellationToken.None); // TODO: usar o CancellationToken para handler
            });

            // Se a política de retry for bem-sucedida, envia o ACK
            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            _logger.LogInformation("Mensagem da fila {QueueName} processada com sucesso (ACK). DeliveryTag: {DeliveryTag}", _queueName, ea.DeliveryTag);
        }
        catch (Exception ex)
        {
            // Se a política de retry falhar, a exceção será capturada aqui
            _logger.LogError(ex, "Erro ao processar mensagem da fila {QueueName}. Enviando para DLQ (NACK). DeliveryTag: {DeliveryTag}", _queueName, ea.DeliveryTag);

            // Rejeita a mensagem (requeue: false) para que ela vá para a DLQ configurada
            await _channel!.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        }
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

            _logger.LogInformation("Topologia do RabbitMQ para a fila '{QueueName}' configurada com sucesso.", queueName);
        }
    }

    private async Task CleanupConnection()
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
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando consumer RabbitMQ...");
        await base.StopAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupConnection();
        GC.SuppressFinalize(this);
    }
}